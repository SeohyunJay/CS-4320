namespace iCAREWebExperiment.Models
{
    public class UserWithRoleViewModel
    {
        public string ID { get; set; }        // Add this if needed for user ID reference
        public string name { get; set; }  // Stores the user's name
        public string userName { get; set; }  // Stores the user's username
        public string email { get; set; }     // Stores the user's email
        public string roleName { get; set; }  // Stores the role's name (e.g., Admin, Doctor, Nurse)
    }
}
