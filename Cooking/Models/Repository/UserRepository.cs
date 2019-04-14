using System.Collections.Generic;
using Cooking.Models.DataModel;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Cooking.Models.Repository
{
	public class UserRepository : BaseRepository
	{
		public UserRepository(IConfiguration config) : base(config) { }
		
		public int Create(User user)
		{
			using (var conn = Connection)
			{
				return conn.ExecuteScalar<int>($@"
insert	[user] (name, password)
values	(@{nameof(user.Name)}, @{nameof(user.Password)})
select	scope_identity()
", user);
			}
		}
		
		public User GetById(int id)
		{
			using (var conn = Connection)
			{
				return conn.QuerySingleOrDefault<User>($@"
select	[user]		{nameof(User.Id)},
		name		{nameof(User.Name)},
		password	{nameof(User.Password)}
from	[user]
where	[user] = @{nameof(id)}
", new { id });
			}
		}
		
		public User GetByName(string name)
		{
			using (var conn = Connection)
			{
				return conn.QuerySingleOrDefault<User>($@"
select	[user]		{nameof(User.Id)},
		name		{nameof(User.Name)},
		password	{nameof(User.Password)}
from	[user]
where	name = @{nameof(name)}
", new { name });
			}
		}
		
		public IEnumerable<User> GetAll()
		{
			using (var conn = Connection)
			{
				return conn.Query<User>($@"
select	[user]		{nameof(User.Id)},
		name		{nameof(User.Name)},
		password	{nameof(User.Password)}
from	[user]
");
			}
		}
		
		public void Update(User user)
		{
			using (var conn = Connection)
			{
				conn.Execute($@"
update	[user]
set		name		= @{nameof(User.Name)},
		password	= @{nameof(User.Password)}
where	[user]		= @{nameof(user.Id)}
", user);
			}
		}
		
		public void Delete(int id)
		{
			using (var conn = Connection)
			{
				conn.Execute($@"
delete	from [user]
where	[user] = @{nameof(id)}
", new { id });
			}
		}
	}
}