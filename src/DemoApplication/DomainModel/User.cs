using System;
using System.Data.Linq.Mapping;

namespace Wintouch.EntityFramework.Demo.DomainModel
{
	// Anemic model to keep this demo application simple.
  [Table]
	public class User
	{
     [Column(
        IsPrimaryKey = true,
        //IsDbGenerated = true,
        AutoSync = AutoSync.OnInsert
    )]
		public Guid Id { get; set; }
    
    [Column]
		public string Name { get; set; }

    [Column]
		public string Email { get; set; }
    
    [Column]
    public int CreditScore { get; set; }
    
    [Column]
    public bool WelcomeEmailSent { get; set; }
    
    [Column]
    public DateTime CreatedOn { get; set; }

		public override string ToString()
		{
			return String.Format("Id: {0} | Name: {1} | Email: {2} | CreditScore: {3} | WelcomeEmailSent: {4} | CreatedOn (UTC): {5}", Id, Name, Email, CreditScore, WelcomeEmailSent, CreatedOn.ToString("dd MMM yyyy - HH:mm:ss"));
		}
	}
}
