using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cooking.Models.Repository
{
	public class BaseRepository
	{
		private readonly IConfiguration _config;

		protected BaseRepository(IConfiguration config)
		{
			_config = config;
		}

		protected IDbConnection Connection => new SqlConnection(_config.GetConnectionString("Default"));
	}
}