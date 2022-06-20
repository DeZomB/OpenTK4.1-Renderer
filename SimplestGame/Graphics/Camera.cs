using OpenTK.Mathematics;

namespace SimplestGame.Graphics;

public class Camera
{
    // radiant rotation around the x axis - called pitch
    public float Pitch;

    // radiant rotation around the y axis - called yaw
    public float Yaw = -90;

    // Field of view
    public float FOV = 90;

    //postion of the Camera
    public Vector3 Postion;

    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }


    public Vector2 Size;

    public Matrix4 View = Matrix4.Identity;
    public Matrix4 Projection = Matrix4.Identity;

    public void Update()
    {
        this.Forward = new()
        {
            X = (float)(Math.Cos(MathHelper.DegreesToRadians(this.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(this.Pitch))),
            Y = (float)Math.Sin(MathHelper.DegreesToRadians(this.Pitch)),
            Z = (float)(Math.Sin(MathHelper.DegreesToRadians(this.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(this.Pitch)))
        };
        // First, the front matrix is calculated using some basic trigonometry.
        // Making sure the vectors are all normalized
        //this.Forward = Vector3.Transform(Vector3.UnitZ, Quaternion.FromEulerAngles(pitch, yaw, 0));

        // Calculate both the right and the up vector using cross product.
        // Calculating the right from the global up
        // This results in a FPS CAMERA
        this.Right = Vector3.Cross(this.Forward, Vector3.UnitY).Normalized();

        // Get the projection matrix
        this.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.FOV), Size.X / Size.Y,
            1, short.MaxValue);
        // Get the view matrix
        this.View = Matrix4.LookAt(this.Postion, this.Postion + this.Forward, Vector3.UnitY);
        //this.View = Matrix4.CreateTranslation(this.Postion) * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(pitch, yaw, 0));

    }
}