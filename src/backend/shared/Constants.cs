namespace Shared;

/// <summary>
/// Various constants used throughout the application
/// </summary>
public static class Constants
{
    /// <summary>
    /// Constants for the api routes
    /// </summary>
    public static class Routes
    {
        /// <summary>
        /// The route to get the data
        /// </summary>
        public const string Data = "/api/data";

        /// <summary>
        /// The route to get the schema
        /// </summary>
        public const string Schema = "/api/schema";

        /// <summary>
        /// The route to get the UI schema
        /// </summary>
        public const string UiSchema = "/api/uischema";

        /// <summary>
        /// The route to get the event handler (client side processing)
        /// </summary>
        public const string EventHandler = "/api/eventhandler";

        /// <summary>
        /// The route for the event handler endpoint (server side processing)
        /// </summary>
        public const string Event = "/api/event";
    }

    /// <summary>
    /// Constants for the file locations
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// File location for the data
        /// </summary>
        public const string Data = "Data/data.json";

        /// <summary>
        /// File location for the schema
        /// </summary>
        public const string Schema = "Data/schema.json";

        /// <summary>
        /// File location for the UI schema
        /// </summary>
        public const string UiSchema = "Data/uischema.json";

        /// <summary>
        /// File location for the event handler code (client side processing)
        /// </summary>
        public const string EventHandler = "Data/eventhandler.js";
    }
}
