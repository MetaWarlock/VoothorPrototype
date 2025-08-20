using System;

[Flags]
public enum PassengerType
{
    Man = 1,
    Woman = 2,
    Elderly = 4
}

public enum PortalPassengerState
{
    Spawning,
    MovingToWaitPoint,
    Waiting,
    MovingToHelicopter,
    Boarding,
    ReturningToWaitPoint,
    EmergeFromHelicopter,
    MovingToPortalDoor,
    InWater,
    SwimmingToHelicopter,
    Drowned
}

public enum PortalMode
{
    SendOnly,
    ReceiveOnly,
    Both
}

public enum DestinationType
{
    Random,
    Specific
}
