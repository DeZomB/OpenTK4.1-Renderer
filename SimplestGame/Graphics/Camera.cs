using OpenTK.Mathematics;

namespace SimplestGame.Graphics;

public class Camera
{
    // Vectors for the camera definition
    private Vector3 cameraForward = -Vector3.UnitZ;
    private Vector3 cameraUp = Vector3.UnitY;
    private Vector3 cameraRight = Vector3.UnitX;

    // radiant rotation around the x axis - called pitch
    private float cameraPitch;

    // radiant rotation around the y axis - called yaw
    private float cameraYaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.

    // Field of view
    private float cameraFOV = MathHelper.PiOver2;

    // camera Constructor
    public Camera(Vector3 position, float aspectRatio)
    {
        this.cameraPosition = position;
        this.cameraAspectratio = aspectRatio;
    }

    //postion of the Camera
    public Vector3 cameraPosition;

    //simple aspectratio
    public float cameraAspectratio;

    public Vector3 Front => this.cameraForward;
    public Vector3 Up => this.cameraUp;
    public Vector3 Right => this.cameraRight;

    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(this.cameraPitch);
        set
        {
            //preventing gimbal lock
            var angle = MathHelper.Clamp(value, -89f, 89f);
            this.cameraPitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(cameraYaw);
        set
        {
            cameraYaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }
    
    // The field of view (FOV) is the vertical angle of the camera view.
    public float FOV
    {
        get => MathHelper.RadiansToDegrees(cameraFOV);
        set
        {
            var angle = MathHelper.Clamp(value, 1f, 90f);
            cameraFOV = MathHelper.DegreesToRadians(angle);
        }
    }

    // Get the view matrix
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(cameraPosition, cameraPosition * this.cameraForward, this.cameraUp);
    }

    // Get Projectionmatrix unsing the same method
    public Matrix4 GetProjectionMatrix()
    {
        //Perspective camera, with clipping depth
        return Matrix4.CreatePerspectiveFieldOfView(this.cameraFOV, cameraAspectratio, 0.01f, 100f);
    }

    private void UpdateVectors()
    {
        // First, the front matrix is calculated using some basic trigonometry.
        cameraForward.X = MathF.Cos(cameraPitch) * MathF.Cos(cameraYaw);
        cameraForward.Y = MathF.Sin(cameraPitch);
        cameraForward.Z = MathF.Cos(cameraPitch) * MathF.Sin(cameraYaw);
        
        // Mmaking sure the vectors are all normalized
        cameraForward = Vector3.Normalize(cameraForward);
        
        // Calculate both the right and the up vector using cross product.
        // Calculating the right from the global up
        // THis results in a FPS CAMERA
        
        cameraRight = Vector3.Normalize(Vector3.Cross(cameraForward, Vector3.UnitY));
        cameraUp = Vector3.Normalize(Vector3.Cross(cameraRight, cameraForward));
    }
}