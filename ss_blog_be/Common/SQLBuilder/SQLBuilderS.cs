using System.Text;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Common.Extensions;

namespace ss_blog_be.Common.SQLBuilder
{
    public class SQLBuilderS : ISQLQueryBuilderMain, ISQLQueryBuilderTable
    {
        SQLITOBuilderCoordinator _coordinator;

        public SQLBuilderS()
        {
            _coordinator = new SQLITOBuilderCoordinator();
        }

        public ISQLQueryBuilderMain Init()
        {
            _coordinator.Init();
            return this;
        }

        public string Build()
        {
            var result = _coordinator.Build();
            _coordinator.Clean();
            return result;
        }


        public ISQLQueryBuilderTable Select(string column, string? alias = null, SQLBuilderFunctions? function = null)
        {
            _coordinator.AddSelect(column, alias, function);
            return this;
        }

        
        public ISQLQueryBuilderTable Where(string column, SQLBuilderOperatorsEnum operatorC, string valueToCompare, SQLBuilderOperatorsEnum logicalOp = SQLBuilderOperatorsEnum.AND)
        {
            _coordinator.AddWhere(column, operatorC, valueToCompare, logicalOp);
            return this;
        }

        public ISQLQueryBuilderTable Where(string column, SQLBuilderOperatorsEnum operatorC, int valueToCompare, SQLBuilderOperatorsEnum logicalOp = SQLBuilderOperatorsEnum.AND)
        {
            _coordinator.AddWhere(column, operatorC, valueToCompare.ToString(), logicalOp);
            return this;
        }

        public ISQLQueryBuilderTable Where(string column, SQLBuilderOperatorsEnum operatorC, long valueToCompare, SQLBuilderOperatorsEnum logicalOp = SQLBuilderOperatorsEnum.AND)
        {
            _coordinator.AddWhere(column, operatorC, valueToCompare.ToString(), logicalOp);
            return this;
        }

        public ISQLQueryBuilderTable Where(string column, SQLBuilderOperatorsEnum operatorC, bool valueToCompare, SQLBuilderOperatorsEnum logicalOp = SQLBuilderOperatorsEnum.AND)
        {
            _coordinator.AddWhere(column, operatorC, valueToCompare.ToInt().ToString(), logicalOp);
            return this;
        }

        ISQLQueryBuilderTable ISQLQueryBuilderTable.From(string table, string? alias)
        {
            _coordinator.SetTableMap(table, alias);
            return this;
        }

        ISQLQueryBuilderTable ISQLQueryBuilderMain.From(string table, string? alias)
        {
            _coordinator.SetTableMap(table, alias);
            return this;
        }

        public ISQLQueryBuilderTable OrderBy(string column, bool desc)
        {
            throw new NotImplementedException();
        }

        ISQLQueryBuilderMain ISQLQueryBuilderMain.Join(string tableA, string tableB, string columnA, string columnB, SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER)
        {
            _coordinator.AddJoin(tableA, tableB, columnA, columnB, joinType);
            return this;
        }

        public ISQLQueryBuilderMain Join(string tableA, string tableB, string columnA, string columnB, SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER)
        {
            _coordinator.AddJoin(tableA, tableB, columnA, columnB, joinType);
            return this;
        }

        public ISQLQueryBuilderMain Join(string tableA, string tableB, string columnA, string columnB, ISQLQueryBuilderMain subQuery, SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER)
        {
            _coordinator.AddJoin(tableA, tableB, columnA, columnB, subQuery, joinType);
            return this;
        }

        public ISQLQueryBuilderMain Join(string tableA, string tableB, string columnA, string columnB, ISQLQueryBuilderTable subQuery, SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER)
        {
            _coordinator.AddJoin(tableA, tableB, columnA, columnB, subQuery as ISQLQueryBuilderMain, joinType);
            return this;
        }

        ISQLQueryBuilderMain ISQLQueryBuilderMain.Limit(int value)
        {
            _coordinator.SetLimit(value);
            return this;
        }

        ISQLQueryBuilderMain ISQLQueryBuilderMain.Offset(int value)
        {
            _coordinator.SetOffSet(value);
            return this;
        }

        public ISQLQueryBuilderMain Distinct()
        {
            throw new NotImplementedException();
        }

        public ISQLQueryBuilderMain GroupBy(string column)
        {
            throw new NotImplementedException();
        }

        

        public ISQLQueryBuilderMain Limit(int value)
        {
            _coordinator.SetLimit(value);
            return this;
        }

        public ISQLQueryBuilderMain Offset(int value)
        {
            _coordinator.SetOffSet(value);
            return this;
        }

        public void AsSubQuery()
        {
            _coordinator.SetAsSubQuery();
        }
    }
}
