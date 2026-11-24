
namespace WebApplication1.Authorization
{
    public static class Permissions
    {
        // FARM
        public const string FarmRead = "Farm.Read";
        public const string FarmCreate = "Farm.Create";
        public const string FarmUpdate = "Farm.Update";
        public const string FarmDelete = "Farm.Delete";

        // FIELD
        public const string FieldRead = "Field.Read";
        public const string FieldCreate = "Field.Create";
        public const string FieldUpdate = "Field.Update";
        public const string FieldDelete = "Field.Delete";

        // CROP
        public const string CropRead = "Crop.Read";
        public const string CropCreate = "Crop.Create";
        public const string CropUpdate = "Crop.Update";
        public const string CropDelete = "Crop.Delete";

        // PLANTING
        public const string PlantingRead = "Planting.Read";
        public const string PlantingCreate = "Planting.Create";
        public const string PlantingUpdate = "Planting.Update";
        public const string PlantingDelete = "Planting.Delete";

        // USER
        public const string UserRead = "User.Read";
        public const string UserCreate = "User.Create";
        public const string UserUpdate = "User.Update";
        public const string UserDelete = "User.Delete";

        public static readonly IReadOnlyCollection<string> All =
        [
            FarmRead, FarmCreate, FarmUpdate, FarmDelete,
            FieldRead, FieldCreate, FieldUpdate, FieldDelete,
            CropRead, CropCreate, CropUpdate, CropDelete,
            PlantingRead, PlantingCreate, PlantingUpdate, PlantingDelete,
            UserRead, UserCreate, UserUpdate, UserDelete
        ];
    }

    public static class RolePermissions
    {
        public static IReadOnlyCollection<string> GetPermissions(string role)
        {
            if (role == "Admin")
                return Permissions.All;

            if (role == "Farmer")
            {
                return
                [
                    Permissions.FarmRead,
                    Permissions.FarmCreate,
                    Permissions.FarmUpdate,

                    Permissions.FieldRead,
                    Permissions.FieldCreate,
                    Permissions.FieldUpdate,
                    Permissions.FieldDelete,

                    Permissions.CropRead,

                    Permissions.PlantingRead,
                    Permissions.PlantingCreate,
                    Permissions.PlantingUpdate,
                    Permissions.PlantingDelete
                ];
            }

            return Array.Empty<string>();
        }
    }
}
