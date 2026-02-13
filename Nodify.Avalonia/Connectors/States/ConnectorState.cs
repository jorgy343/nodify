namespace Nodify.Interactivity;

public static partial class ConnectorState
{
    /// <summary>
    /// Determines whether toggled connecting mode is enabled, allowing the user to start and end the interaction in two steps with the same input gesture.
    /// </summary>
    public static bool EnableToggledConnectingMode { get; set; }

    // Note: Full implementation of state handlers requires the Interactivity system to be ported
    // The following methods are placeholders for when the InputProcessor system is available

    internal static void RegisterDefaultHandlers()
    {
        // TODO: Port InputProcessor system and implement state handlers
        // InputProcessor.Shared<Connector>.RegisterHandlerFactory(elem => new Disconnect(elem));
        // InputProcessor.Shared<Connector>.RegisterHandlerFactory(elem => new Connecting(elem));
        // InputProcessor.Shared<Connector>.RegisterHandlerFactory(elem => new Default(elem));
    }
}
