using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using TUIO;

public class TuioDemo : Form, TuioListener
{
    private TuioClient client;
    private Dictionary<long, TuioObject> objectList;
    private Dictionary<long, TuioCursor> cursorList;
    private Dictionary<long, TuioBlob> blobList;

    // Screen dimensions
    public static int width, height;
    private int screen_width = Screen.PrimaryScreen.Bounds.Width;
    private int screen_height = Screen.PrimaryScreen.Bounds.Height;

    private bool fullscreen;
    private bool verbose;

    // Brushes and Pens
    SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(189, 217, 229));

    SolidBrush fntBrush = new SolidBrush(Color.Black); // Black text for visibility on white background
    SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
    SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
    Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);

    // Images
    private Image back; // Background image for the form
    private Image ID_0; // Image for SymbolID == 0 (Male)
    private Image ID_1;  // Image for SymbolID == 1 (Female)
    private Image ID_2; // Image for SymbolID == 2 (pants)
    private Image ID_3; //// Image for SymbolID == 3 (shirt)
    private Image leftOfMale; // New image to the left of the male character
    private Image rightOfFemale; // New image to the right of the female character

 

    // Display flag
    private int? currentDisplayedSymbolID = null; // Current SymbolID being displayed

    // Fixed positions for images
    private Point fixedPositionZero; // Initial position for SymbolID == 0 (left)
    private Point fixedPositionOne;  // Initial position for SymbolID == 1 (right)

    public TuioDemo(int port)
    {
        //verbose = false;
        fullscreen = true; // Start in fullscreen
        width = screen_width;
        height = screen_height;

        // Set the window to maximized state
        this.WindowState = FormWindowState.Maximized;
        this.FormBorderStyle = FormBorderStyle.None; // Remove borders for fullscreen

        this.ClientSize = new System.Drawing.Size(width, height);

        // Event handlers
        this.Closing += new CancelEventHandler(Form_Closing);
        this.KeyDown += new KeyEventHandler(Form_KeyDown);

        // Enable double buffering to reduce flicker
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.UserPaint |
                      ControlStyles.DoubleBuffer, true);

        // Initialize dictionaries
        objectList = new Dictionary<long, TuioObject>(128);
        cursorList = new Dictionary<long, TuioCursor>(128);
        blobList = new Dictionary<long, TuioBlob>(128);

        // Initialize and connect TUIO client
        client = new TuioClient(port);
        client.addTuioListener(this);
        client.connect();

        // Load the background image
        try
        {
            back = Image.FromFile("back3.JPG"); // Provide the correct path to your background image
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading background image: " + ex.Message);
            back = null; // Ensure it's null if loading fails
        }

        // Load the images for SymbolID 0 (Male) and 1 (Female)
        try
        {
            ID_0 = Image.FromFile("male-1.PNG"); // Provide path for SymbolID 0
            ID_1 = Image.FromFile("female-1.PNG"); // Provide path for SymbolID 1
            ID_2 = Image.FromFile("pants1.png"); // Provide path for SymbolID 2
            ID_3 = Image.FromFile("shirt1.png"); // Provide path for SymbolID 3
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading image: " + ex.Message);
        }

     

        // Load the two new images
        try
        {
            leftOfMale = Image.FromFile("maleop.PNG"); // Image on the left of the male character
            rightOfFemale = Image.FromFile("femaleop.PNG"); // Image on the right of the female character
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading side images: " + ex.Message);
        }

      

        // Set fixed positions for male and female images (left and right of the screen)
        fixedPositionZero = new Point(650, height / 2); // Position for male image on the left
        fixedPositionOne = new Point(800, height / 2);  // Position for female image on the right
    }

    private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyData == Keys.F1)
        {
            //ToggleFullscreen();
        }
        else if (e.KeyData == Keys.Escape)
        {
            this.Close();
        }
    }

    private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        client.removeTuioListener(this);
        client.disconnect();
        System.Environment.Exit(0);
    }

    public void addTuioObject(TuioObject o)
    {
        if (o.SymbolID == 0 && ID_0 != null)
        {
            currentDisplayedSymbolID = 0;
            Invalidate(); // Redraw to move the male image
        }
        else if (o.SymbolID == 1 && ID_1 != null)
        {
            currentDisplayedSymbolID = 1;
            Invalidate(); // Redraw to move the female image
        }
    }

    public void updateTuioObject(TuioObject o)
    {
        if (verbose)
            Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);
    }

    public void removeTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList.Remove(o.SessionID);
        }
        if (verbose)
            Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
    }

    public void refresh(TuioTime frameTime)
    {
        Invalidate(); // Trigger a repaint
    }

    // Implement missing TuioListener interface methods (stubs for now)
    public void addTuioCursor(TuioCursor c) { }
    public void updateTuioCursor(TuioCursor c) { }
    public void removeTuioCursor(TuioCursor c) { }
    public void addTuioBlob(TuioBlob b) { }
    public void updateTuioBlob(TuioBlob b) { }
    public void removeTuioBlob(TuioBlob b) { }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        Graphics g = pevent.Graphics;

        // Fill the background with a solid color
        g.Clear(Color.FromArgb(189, 217, 229));

        // Draw the background image if available
        if (back != null)
        {
             g.DrawImage(back, new Rectangle(0, 0, width, height));
        }
        else
        {
            // Fallback to solid color if background image is not available
            g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));
        }


        // Draw male and female images in their initial positions
        if (currentDisplayedSymbolID == null)
        {
            if (ID_0 != null)
            {
                // Draw the image to the left of the male character
                if (leftOfMale != null)
                {
                    g.DrawImage(leftOfMale, fixedPositionZero.X - 150, fixedPositionZero.Y+150, 170, 70); // Left of male
                }

                g.DrawImage(ID_0, fixedPositionZero.X, fixedPositionZero.Y-50, 120, 447); // Draw male image
            }

            if (ID_1 != null)
            {
                g.DrawImage(ID_1, fixedPositionOne.X, fixedPositionOne.Y-50, 120, 447); // Draw female image

                // Draw the image to the right of the female character
                if (rightOfFemale != null)
                {
                    g.DrawImage(rightOfFemale, fixedPositionOne.X + 95, fixedPositionOne.Y+150, 170, 70); // Right of female
                }
            }
        }

        // Move the male image to the center and hide the female
        if (currentDisplayedSymbolID == 0 && ID_0 != null)
        {
            g.DrawImage(ID_0, width / 2 - 50, height / 2, 120, 447); // Move male image to the center
            g.DrawImage(ID_2, width / 2 - 150, height / 2, 120, 447); // Move male image to the center
            g.DrawImage(ID_3, width / 2+ 150, height / 2, 120, 447); // Move male image to the center

        }

        // Move the female image to the center and hide the male
        if (currentDisplayedSymbolID == 1 && ID_1 != null)
        {
            g.DrawImage(ID_1, width / 2 - 50, height / 2, 120, 447); // Move female image to the center
        }
    }

    public static void Main(string[] argv)
    {
        int port = 3333;
        if (argv.Length == 1)
        {
            port = int.Parse(argv[0]);
        }
        Application.Run(new TuioDemo(port));
    }
}
