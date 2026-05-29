namespace EcommerceApp.Models.Database
{
    public class User
    {
        private User()
        {

        }
        public User(string fName, string lName, string email, string pswd)
        {
            Id = Guid.NewGuid().ToString();
            FirstName = fName;
            LastName = lName;
            Email = email;
            Password = pswd;
            Role = AppUserRoles.Basic;
        }

        public string Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public AppUserRoles Role { get; private set; }
    }

    public enum AppUserRoles {
        Basic,
        Admin
    }
}
