using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text;
using ss_blog_be.Common.SQLBuilder.Enums;

namespace ss_blog_be.Common.SQLBuilder
{
    public class SQLITOBuilderCoordinator
    {
        bool _asSubQuery; 

        SQLBuilderTableMap _firstTableDeclared;
        SQLBuilderTableMap _currentTable;
        SQLBuilderColumnMap _currentColumn;

        [Required] private StringBuilder _stringBuilder;

        [Required] private IDictionary<string, SQLBuilderTableMap> _tableMaps;

        [Required] private ICollection<JoinClause> _Joins;

        private int? _offSet;
        private int? _limit;

        public SQLITOBuilderCoordinator()
        {
            _stringBuilder = new StringBuilder();
            _tableMaps = new Dictionary<string, SQLBuilderTableMap>();
            _Joins = new HashSet<JoinClause>();
            _asSubQuery = false;
        }

        public void Init()
        {
            _stringBuilder.Clear();
        }

        public void SetOffSet(int offset)
        {
            _offSet = offset;
        }

        public void SetLimit(int limit)
        {
            _limit = limit;
        }

        public void SetAsSubQuery()
        {
            _asSubQuery = true;
        }

        public void Clean()
        {
            _tableMaps.Clear();
            _currentTable = null;
            _stringBuilder.Clear();
        }

        public void AddJoin([Required] string tableA, [Required] string tableB, 
                            [Required] string columnA, [Required] string columnB, 
                            SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER)
        {
            AddTableIfDoesntExists(tableA);
            AddTableIfDoesntExists(tableB);
            _Joins.Add(new JoinClause() { TableA = tableA, TableB = tableB, ColumA = columnA, ColumB = columnB, JoinType = joinType, IsSubQuery = false });
        }



        public void AddJoin([Required] string tableA, [Required] string tableB,
                            [Required] string columnA, [Required] string columnB, 
                            [Required] ISQLQueryBuilderMain subQuery,
                            SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER)
        {
            AddTableIfDoesntExists(tableA);
            AddTableIfDoesntExists(tableB);
            subQuery.AsSubQuery();
            _Joins.Add(new JoinClause() { TableA = tableA, TableB = tableB, ColumA = columnA, ColumB = columnB, SubQuery = subQuery, JoinType = joinType, IsSubQuery=true });
        }

        public void AddWhere(string column, SQLBuilderOperatorsEnum operatorC, 
                            string valueToCompare, SQLBuilderOperatorsEnum aggregationOp = SQLBuilderOperatorsEnum.AND) 
        {
            SetColumn(column, null, false);
            _currentColumn.WhereClauses.Add(new WhereClause() { LogicalOp = operatorC, Value = valueToCompare, AggregationOp = aggregationOp });            
        }

        public void AddSelect(string column, string? alias = null, SQLBuilderFunctions? function = null)
        {
            SetColumn(column, alias, true);

            _currentColumn.IsSelected = true;
            _currentColumn.Alias = alias;
            _currentColumn.Functions = function;

        }

        public void AddSelectFunction(string column, string? alias = null)
        {
            SetColumn(column, alias, true);

            _currentColumn.IsSelected = true;
            _currentColumn.Alias = alias;
        }

        public void SetColumn(string column, string? alias = null, bool isSelected = false)
        {
            AddColumnIfDoesntExists(column, alias, isSelected);
            _currentColumn = _currentTable.Columns[column];
        }

        public void SetTableMap(string table, string? alias = null) 
        {
            AddTableIfDoesntExists(table, alias);
            _currentTable = _tableMaps[table];
        }

        private void AddColumnIfDoesntExists([Required] string column, string? alias = null, bool isSelected = false)
        {
            if (_currentTable == null) throw new SQLBuilderException("Please call SetTableMap before executing this method");

            if (!_currentTable.Columns.ContainsKey(column))
            {
                _currentTable.Columns.Add(column, new SQLBuilderColumnMap(alias, isSelected));
            }

        }

        public void AddTableIfDoesntExists([Required] string table, string? alias = null)
        {
            if (!_tableMaps.ContainsKey(table)) 
            {
                var isFirst = _tableMaps.Count == 0;
                var _alias = string.IsNullOrEmpty(alias)? "t" + (_tableMaps.Keys.Count + 1) : alias;
                var data = new SQLBuilderTableMap(_alias, isFirst, table);

                if(isFirst) _firstTableDeclared = data;

                _tableMaps.Add(table, data);
            }
        }

        public string Build() 
        {
            if (_asSubQuery) _stringBuilder.Append("(");
            _stringBuilder.Append(SQLBuilderSConstants.SELECT);
            
            buildSelect();
            buildFrom();
            buildJoins();
            buildWhere();
            addLimit();
            addOFFSet();

            if (_asSubQuery) _stringBuilder.Append(")");
            if (!_asSubQuery) _stringBuilder.Append(";");
            return _stringBuilder.ToString();
        }

        private void buildSelect()
        {

            foreach (var k in _tableMaps.Keys)
            {
                var tab = _tableMaps[k];

                foreach (var c in _tableMaps[k].Columns)
                {
                    if (c.Value.IsSelected)
                    {
                        if (c.Value.Functions.HasValue)
                        {
                            _stringBuilder.Append(c.Value.Functions.Value.ToString());
                            _stringBuilder.Append("(");
                            _stringBuilder.Append(c.Key);
                            _stringBuilder.Append(")");

                        }
                        else
                        {
                            _stringBuilder.Append(tab.Alias);
                            _stringBuilder.Append(SQLBuilderSConstants.Point);
                            _stringBuilder.Append(c.Key);

                        }

                        if (!string.IsNullOrEmpty(c.Value.Alias))
                        {
                            _stringBuilder.Append(SQLBuilderSConstants.AS);
                            _stringBuilder.Append(c.Value.Alias);
                        }

                        _stringBuilder.Append(SQLBuilderSConstants.Comma);

                    }
                }
            }

            _stringBuilder.Remove(_stringBuilder.Length - 2, 2);
        }

        private void buildFrom()
        {

            _stringBuilder.Append(SQLBuilderSConstants.FROM);
            _stringBuilder.Append(_firstTableDeclared.Name);
            _stringBuilder.Append(SQLBuilderSConstants.AS);
            _stringBuilder.Append(_firstTableDeclared.Alias);

        }

        private void buildWhere()
        {
            

            var isFirst = true;

            foreach (var k in _tableMaps.Keys)
            {
                var tab = _tableMaps[k];

                foreach (var c in _tableMaps[k].Columns)
                {
                    
                        foreach (var w in c.Value.WhereClauses)
                        {
                            if (isFirst)
                            {
                            _stringBuilder.Append(SQLBuilderSConstants.WHERE);
                        }
                            
                            if(!isFirst)
                            {
                                _stringBuilder.Append(SQLBuilderEnumsTo.GetOperatorAsStr(w.AggregationOp == SQLBuilderOperatorsEnum.AND ? w.AggregationOp : SQLBuilderOperatorsEnum.OR));
                            }

                            _stringBuilder.Append(tab.Alias);
                            _stringBuilder.Append(SQLBuilderSConstants.Point);
                            _stringBuilder.Append(c.Key);
                            _stringBuilder.Append(SQLBuilderEnumsTo.GetOperatorAsStr(w.LogicalOp));
                            _stringBuilder.Append(w.Value);


                            isFirst = false;
                        }
                    
                }
            }
        }

        private void buildJoins()
        {
            foreach (var wj in _Joins)
            {
                var tableDataA = _tableMaps[wj.TableA];
                var tableDataB = _tableMaps[wj.TableB];

                    _stringBuilder.Append(SQLBuilderEnumsTo.GetJoinTypeAsStr(wj.JoinType));
                    _stringBuilder.Append(tableDataB.Name);
                    _stringBuilder.Append(SQLBuilderSConstants.AS);
                    _stringBuilder.Append(tableDataB.Alias);
                    _stringBuilder.Append(SQLBuilderSConstants.ON);

                    _stringBuilder.Append(tableDataB.Alias);
                    _stringBuilder.Append(SQLBuilderSConstants.Point);
                    _stringBuilder.Append(wj.ColumB);

                    _stringBuilder.Append(SQLBuilderEnumsTo.GetOperatorAsStr(SQLBuilderOperatorsEnum.EQUAL));

                if (wj.IsSubQuery)
                {
                    _stringBuilder.Append(wj.SubQuery.Build());
                }
                else
                {
                    _stringBuilder.Append(tableDataA.Alias);
                    _stringBuilder.Append(SQLBuilderSConstants.Point);
                    _stringBuilder.Append(wj.ColumA);
                }
            }
        }

        private void addOFFSet() 
        {
            if (_offSet.HasValue)
            {
                _stringBuilder.Append(SQLBuilderSConstants.OFFSET);
                _stringBuilder.Append(_offSet.Value);
            }
        }

        private void addLimit()
        {
            if (_limit.HasValue)
            {
                _stringBuilder.Append(SQLBuilderSConstants.LIMIT);
                _stringBuilder.Append(_limit.Value);
            }
        }
    }
}
