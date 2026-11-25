
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

        // HARVEST
        public const string HarvestRead = "Harvest.Read";
        public const string HarvestCreate = "Harvest.Create";
        public const string HarvestUpdate = "Harvest.Update";
        public const string HarvestDelete = "Harvest.Delete";

        // CARE SCHEDULE
        public const string CareScheduleRead = "CareSchedule.Read";
        public const string CareScheduleCreate = "CareSchedule.Create";
        public const string CareScheduleUpdate = "CareSchedule.Update";
        public const string CareScheduleDelete = "CareSchedule.Delete";

        // PEST INCIDENT
        public const string PestIncidentRead = "PestIncident.Read";
        public const string PestIncidentCreate = "PestIncident.Create";
        public const string PestIncidentUpdate = "PestIncident.Update";
        public const string PestIncidentDelete = "PestIncident.Delete";

        // DASHBOARD
        public const string DashboardRead = "Dashboard.Read";

        // IOT
        public const string SensorDeviceRead = "SensorDevice.Read";
        public const string SensorDeviceCreate = "SensorDevice.Create";
        public const string SensorDeviceUpdate = "SensorDevice.Update";
        public const string SensorDeviceDelete = "SensorDevice.Delete";
        public const string SensorReadingRead = "SensorReading.Read";
        public const string AlertRuleRead = "AlertRule.Read";
        public const string AlertRuleCreate = "AlertRule.Create";
        public const string AlertRuleUpdate = "AlertRule.Update";
        public const string AlertRuleDelete = "AlertRule.Delete";
        public const string AlertRead = "Alert.Read";
        public const string AlertUpdate = "Alert.Update";
        public const string AlertDelete = "Alert.Delete";

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
            HarvestRead, HarvestCreate, HarvestUpdate, HarvestDelete,
            CareScheduleRead, CareScheduleCreate, CareScheduleUpdate, CareScheduleDelete,
            PestIncidentRead, PestIncidentCreate, PestIncidentUpdate, PestIncidentDelete,
            DashboardRead,
            SensorDeviceRead, SensorDeviceCreate, SensorDeviceUpdate, SensorDeviceDelete,
            SensorReadingRead,
            AlertRuleRead, AlertRuleCreate, AlertRuleUpdate, AlertRuleDelete,
            AlertRead, AlertUpdate, AlertDelete,
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
                    Permissions.PlantingDelete,

                    Permissions.HarvestRead,
                    Permissions.HarvestCreate,
                    Permissions.HarvestUpdate,
                    Permissions.CareScheduleRead,
                    Permissions.CareScheduleCreate,
                    Permissions.CareScheduleUpdate,
                    Permissions.CareScheduleDelete,
                    Permissions.PestIncidentRead,
                    Permissions.PestIncidentCreate,
                    Permissions.PestIncidentUpdate,
                    Permissions.DashboardRead,
                    Permissions.SensorDeviceRead,
                    Permissions.SensorReadingRead,
                    Permissions.AlertRuleRead,
                    Permissions.AlertRuleCreate,
                    Permissions.AlertRuleUpdate,
                    Permissions.AlertRead,
                    Permissions.AlertUpdate
                ];
            }

            return Array.Empty<string>();
        }
    }
}
