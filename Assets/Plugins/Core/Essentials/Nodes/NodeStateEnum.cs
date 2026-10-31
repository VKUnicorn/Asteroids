namespace Core {
    public enum NodeStateEnum {
        Created,
        Constructed,  // Constructed, awaiting Inject
        Injected,     // Injected, awaiting Activation
        Active,       // Activated, enabled
        Inactive      // Deactivated, disabled
    }
}