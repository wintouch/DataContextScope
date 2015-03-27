using System;
using System.Threading.Tasks;
using Wintouch.EntityFramework.Demo.DomainModel;

namespace Wintouch.EntityFramework.Demo.Repositories
{
	public interface IUserRepository 
	{
		User Get(Guid userId);
		void Add(User user);
	}
}