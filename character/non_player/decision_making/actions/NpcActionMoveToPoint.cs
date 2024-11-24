namespace SneakGame;

using Godot;

public partial class NpcActionUtilities
{

    public static bool MoveToPoint(NonPlayerBrain owner, float movementSpeed = 1)
    {
        // Calculate the velocity towards the target position
        Vector3 velocity = owner.TargetPosition - owner.NonPlayer.GlobalTransform.Origin;

        // Cap the velocity to WalkSpeed
        if (velocity.Length() > movementSpeed)
        {
            velocity = velocity.Normalized() * movementSpeed;
        }

        owner.NonPlayer.Velocity = velocity;

        // Face the target position if not already facing it
        if (owner.NonPlayer.Velocity.Length() > 0.1f)
        {
            Vector3 direction = owner.NonPlayer.Position - owner.TargetPosition;
            owner.NonPlayer.LookAt(owner.NonPlayer.Position + direction, Vector3.Up);
        }

        // Apply movement
        owner.NonPlayer.MoveAndSlide();

        return true;
    }

    public static void GetNewRandomPositionOnMap(NonPlayerBrain owner)
    {
        // Get a random point between -50 and 50 on the x and z axis
        owner.TargetPosition = new Vector3(
            GD.RandRange(-50, 50),
            0,
            GD.RandRange(-50, 50)
        );
        GD.Print("New target position: " + owner.TargetPosition);
    }

}