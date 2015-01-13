using System;
using System.Threading.Tasks;
using Numero3.EntityFramework.Demo.DomainModel;

namespace Numero3.EntityFramework.Demo.Repositories
{
	public interface IUserRepository 
	{
		User Get(Guid userId);
		void Add(User user);
	}
}