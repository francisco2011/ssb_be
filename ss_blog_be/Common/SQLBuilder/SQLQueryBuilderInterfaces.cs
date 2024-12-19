using System.Numerics;
using ss_blog_be.Common.SQLBuilder.Enums;

namespace ss_blog_be.Common.SQLBuilder
{
    public interface ISQLQueryBuilderMain
    {
        ISQLQueryBuilderMain Init();
        ISQLQueryBuilderTable From(string table, string? alias = null);
        ISQLQueryBuilderMain Join(string tableA, string tableB, string columnA, string columnB, SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER);
        ISQLQueryBuilderMain Join(string tableA, string tableB, string columnA, string columnB, ISQLQueryBuilderMain subQuery, SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER);
        ISQLQueryBuilderMain Limit(int value);
        ISQLQueryBuilderMain Offset(int value);
        ISQLQueryBuilderMain Distinct();
        void AsSubQuery();
        string Build();

    }

    public interface ISQLQueryBuilderTable
    {
        ISQLQueryBuilderTable From(string table, string? alias = null);
        ISQLQueryBuilderTable Select(string column, string? alias = null, SQLBuilderFunctions? function = null);
        ISQLQueryBuilderMain GroupBy(string column);
        ISQLQueryBuilderTable OrderBy(string column, bool desc);
        ISQLQueryBuilderTable Where(string column, SQLBuilderOperatorsEnum operatorC, string valueToCompare, SQLBuilderOperatorsEnum logicalOp = SQLBuilderOperatorsEnum.AND);
        ISQLQueryBuilderTable Where(string column, SQLBuilderOperatorsEnum operatorC, int valueToCompare, SQLBuilderOperatorsEnum logicalOp = SQLBuilderOperatorsEnum.AND);
        ISQLQueryBuilderTable Where(string column, SQLBuilderOperatorsEnum operatorC, long valueToCompare, SQLBuilderOperatorsEnum logicalOp = SQLBuilderOperatorsEnum.AND);
        ISQLQueryBuilderTable Where(string column, SQLBuilderOperatorsEnum operatorC, bool valueToCompare, SQLBuilderOperatorsEnum logicalOp = SQLBuilderOperatorsEnum.AND);
        ISQLQueryBuilderMain Join(string tableA, string tableB, string columnA, string columnB, SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER);
        ISQLQueryBuilderMain Join(string tableA, string tableB, string columnA, string columnB, ISQLQueryBuilderTable subQuery, SQLBuilderJoinTypeEnum joinType = SQLBuilderJoinTypeEnum.INNER);
        ISQLQueryBuilderMain Limit(int value);
        ISQLQueryBuilderMain Offset(int value);
        ISQLQueryBuilderMain Distinct();
        void AsSubQuery();
        string Build();
    }
}
