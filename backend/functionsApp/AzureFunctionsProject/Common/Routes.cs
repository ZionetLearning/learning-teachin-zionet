namespace AzureFunctionsProject.Common
{
    /// <summary>
    /// Centralized route templates for all HTTP‑triggered functions.
    /// </summary>
    public static class Routes
    {
        // Accessor functions
        public const string AccessorGetAll = "accessor/data";
        public const string AccessorGetById = "accessor/data/{id}";
        public const string AccessorCreate = "accessor/data";
        public const string AccessorUpdate = "accessor/data/{id}";
        public const string AccessorDelete = "accessor/data/{id}";

        // Manager functions
        public const string ManagerGetAll = "data";
        public const string ManagerGetById = "data/{id}";
        public const string ManagerCreate = "data";
        public const string ManagerUpdate = "data/{id}";
        public const string ManagerDelete = "data/{id}";
    }
}
