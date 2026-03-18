using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace ss_blog_be.Services.Data
{
    public class UOW
    {
        private SqliteConnection _conn { get; }
        private DbTransaction _currentTx;

        public TagDataService TagService { get; }
        public PostDataService PostDataService { get; }
        public PostTypeDataService PostTypeService { get; }

        public UOW([Required] SqliteConnection conn)
        {
            _conn = conn;
            TagService = new TagDataService(conn);
            PostDataService = new PostDataService(conn);
            PostTypeService = new PostTypeDataService(conn);
        }

        public async Task BeginTransaction()
        {
            if (_currentTx != null) throw new Exception("A previous transactions is running");

            _currentTx = (await _conn.BeginTransactionAsync());
        }

        public async Task CommitTransaction()
        {
           await _currentTx.CommitAsync();
            DisposeTx(); 
        }

        public async Task RollbackTransaction()
        {
            await _currentTx.RollbackAsync();
            DisposeTx();
        }

        private void DisposeTx()
        {
            if (_currentTx != null)
            {
                _currentTx.Dispose();
                _currentTx = null;
            }
            
        }


    }
}
