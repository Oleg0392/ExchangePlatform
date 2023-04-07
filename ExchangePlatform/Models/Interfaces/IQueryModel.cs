using Microsoft.Data.SqlClient;

namespace ExchangePlatform.Models.Interfaces
{
    public interface IQueryModel
    {
        public SqlCommand GetInsertCommand(int Id = 0);
        public SqlCommand GetSelectCommand(int Id = 0);
        public SqlCommand GetUpdateCommand(int Id, object NewValue);
        public SqlCommand GetDeleteCommand(int Id = 0);

        //public static SqlCommand GetSelectAllCommand();
    }
}
