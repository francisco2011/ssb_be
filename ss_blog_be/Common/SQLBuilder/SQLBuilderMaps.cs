using ss_blog_be.Common.SQLBuilder.Enums;

namespace ss_blog_be.Common.SQLBuilder
{
    public class SQLBuilderTableMap
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool FirstDeclared { get; set; }
        public IDictionary<string, SQLBuilderColumnMap> Columns { get; set; }

        public SQLBuilderTableMap(string alias, bool firstDeclared, string name)
        {
            Columns = new Dictionary<string, SQLBuilderColumnMap>();
            Alias = alias;
            FirstDeclared = firstDeclared;
            Name = name;
        }

    }

    public class SQLBuilderColumnMap
    {
        public string? Alias { get; set; }
        public bool IsSelected { get; set; }
        public bool OrderBy { get; set; }
        public bool isOrderDesc { get; set; }
        public SQLBuilderFunctions? Functions { get; set; }
        public ICollection<WhereClause> WhereClauses { get; }

        public SQLBuilderColumnMap(string? alias, bool isSelected = false)
        {
            IsSelected = isSelected;
            Alias = alias;
            WhereClauses = new List<WhereClause>();
        }
    }

    public class WhereClause
    {
        public string Value { get; set; }
        public SQLBuilderOperatorsEnum LogicalOp { get; set; }
        public SQLBuilderOperatorsEnum AggregationOp { get; set; }
        public WhereClause() { }
    }

    public class JoinClause
    {
        public SQLBuilderJoinTypeEnum JoinType { get; set; }
        public string TableA { get; set; }
        public string TableB { get; set; }
        public string ColumA { get; set; }
        public string ColumB { get; set; }
        
        public bool IsSubQuery { get; set; }
        public ISQLQueryBuilderMain SubQuery { get; set; }

        public JoinClause() { }
    }
}
