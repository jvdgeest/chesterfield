
namespace Chesterfield.Interfaces
{
	public interface IAuditableDocument
	{
		void Creating();
		void Updating();
		void Deleting();

		void Created();
		void Updated();
		void Deleted();
	}
}
