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
    private Image leftOfMale; // New image to the left of the male character
    private Image rightOfFemale; // New image to the right of the female character

    // New Images for SymbolIDs 20-25 ("Rate This Outfit" and others)
    private Image rateOutfitImage;
    private Image rateOutfitImage21;
    private Image rateOutfitImage22;
    private Image rateOutfitImage23;
    private Image rateOutfitImage24;
    private Image rateOutfitImage25;

    // New Images for SymbolIDs 2 through 7
    private Image ID_2;
    private Image ID_3;
    private Image ID_4;
    private Image ID_5;
    private Image ID_6;
    private Image ID_7;

    // Display flag
    private int? currentDisplayedSymbolID = null; // Current SymbolID being displayed

    // Fixed positions for images
    private Point fixedPositionZero; // Initial position for SymbolID == 0 (left)
    private Point fixedPositionOne;  // Initial position for SymbolID == 1 (right)

    // Rectangle dimensions for SymbolID == 20-25 ("Rate This Outfit" and others)
    private Rectangle rateOutfitBox = new Rectangle(200, 300, 50, 150); // Slightly adjusted box size

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

        // Load the images for SymbolID 20-25 ("Rate This Outfit" variations)
        try
        {
            rateOutfitImage = Image.FromFile("0.PNG"); // Provide path for the image
            rateOutfitImage21 = Image.FromFile("1.PNG");
            rateOutfitImage22 = Image.FromFile("2.PNG");
            rateOutfitImage23 = Image.FromFile("3.PNG");
            rateOutfitImage24 = Image.FromFile("4.PNG");
            rateOutfitImage25 = Image.FromFile("5.PNG");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading outfit images: " + ex.Message);
        }

        // Load the images for SymbolID 2 through 7
        try
        {
            ID_2 = Image.FromFile("outm-1.PNG"); // Provide path for SymbolID 2
            ID_3 = Image.FromFile("outm-2.PNG"); // Provide path for SymbolID 3
            ID_4 = Image.FromFile("outm-3.PNG"); // Provide path for SymbolID 4
            ID_5 = Image.FromFile("outf-1.PNG"); // Provide path for SymbolID 5
            ID_6 = Image.FromFile("outf-2.PNG"); // Provide path for SymbolID 6
            ID_7 = Image.FromFile("outf-3.PNG"); // Provide path for SymbolID 7
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading image for SymbolIDs 2-7: " + ex.Message);
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
        else if (o.SymbolID >= 20 && o.SymbolID <= 25) // Check for SymbolID between 20 and 25
        {
            currentDisplayedSymbolID = (int)o.SymbolID;
            Invalidate(); // Redraw to display the "Rate This Outfit" box
        }
        else if (o.SymbolID == 2 && ID_2 != null)
        {
            currentDisplayedSymbolID = 2;
            Invalidate(); // Redraw to move the image
        }
        else if (o.SymbolID == 3 && ID_3 != null)
        {
            currentDisplayedSymbolID = 3;
            Invalidate(); // Redraw to move the image
        }
        else if (o.SymbolID == 4 && ID_4 != null)
        {
            currentDisplayedSymbolID = 4;
            Invalidate(); // Redraw to move the image
        }
        else if (o.SymbolID == 5 && ID_5 != null)
        {
            currentDisplayedSymbolID = 5;
            Invalidate(); // Redraw to move the image
        }
        else if (o.SymbolID == 6 && ID_6 != null)
        {
            currentDisplayedSymbolID = 6;
            Invalidate(); // Redraw to move the image
        }
        else if (o.SymbolID == 7 && ID_7 != null)
        {
            currentDisplayedSymbolID = 7;
            Invalidate(); // Redraw to move the image
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
                    g.DrawImage(leftOfMale, fixedPositionZero.X - 150, fixedPositionZero.Y + 150, 170, 70); // Left of male
                }

                g.DrawImage(ID_0, fixedPositionZero.X, fixedPositionZero.Y - 50, 120, 447); // Draw male image
            }

            if (ID_1 != null)
            {
                g.DrawImage(ID_1, fixedPositionOne.X, fixedPositionOne.Y - 50, 120, 447); // Draw female image

                // Draw the image to the right of the female character
                if (rightOfFemale != null)
                {
                    g.DrawImage(rightOfFemale, fixedPositionOne.X + 95, fixedPositionOne.Y + 150, 170, 70); // Right of female
                }
            }
        }

        // Move the male image to the center and hide the female
        if (currentDisplayedSymbolID == 0 && ID_0 != null)
        {
            g.DrawImage(ID_0, width / 2 - 50, height / 2, 120, 447); // Move male image to the center
        }

        // Move the female image to the center and hide the male
        if (currentDisplayedSymbolID == 1 && ID_1 != null)
        {
            g.DrawImage(ID_1, width / 2 - 50, height / 2, 120, 447); // Move female image to the center
        }

        // Show the "Rate This Outfit" box when SymbolID == 20-25
        if (currentDisplayedSymbolID >= 20 && currentDisplayedSymbolID <= 25)
        {
            Image outfitImage = null;
            switch (currentDisplayedSymbolID)
            {
                case 20:
                    outfitImage = rateOutfitImage;
                    break;
                case 21:
                    outfitImage = rateOutfitImage21;
                    break;
                case 22:
                    outfitImage = rateOutfitImage22;
                    break;
                case 23:
                    outfitImage = rateOutfitImage23;
                    break;
                case 24:
                    outfitImage = rateOutfitImage24;
                    break;
                case 25:
                    outfitImage = rateOutfitImage25;
                    break;
            }

            // Adjust the width and height for scaling the image down
            int newWidth = outfitImage.Width / 2;  // Example: scale down to half the size
            int newHeight = outfitImage.Height / 2;  // Example: scale down to half the size

            // Calculate the center of the screen
            int centerX = (width / 2) - (newWidth / 2);
            int centerY = (height / 2) - (newHeight / 2);

            // Draw the image at the center of the screen with the new size
            g.DrawImage(outfitImage, centerX, centerY, newWidth, newHeight);

            // Draw the text at a position relative to the image
            g.DrawString("Rate This Outfit", new Font("Arial", 16), Brushes.Black, new Point(centerX + 100, centerY - 30));
        }

        // Logic to display SymbolIDs 2-7
        if (currentDisplayedSymbolID == 2 && ID_2 != null)
        {
            g.DrawImage(ID_2, width / 2 - 50, height / 2 - 15, 120, 447); // Move image to the center
        }
        else if (currentDisplayedSymbolID == 3 && ID_3 != null)
        {
            g.DrawImage(ID_3, width / 2 - 50, height / 2 - 15, 120, 447); // Move image to the center
        }
        else if (currentDisplayedSymbolID == 4 && ID_4 != null)
        {
            g.DrawImage(ID_4, width / 2 - 50, height / 2 - 15, 120, 447); // Move image to the center
        }
        else if (currentDisplayedSymbolID == 5 && ID_5 != null)
        {
            g.DrawImage(ID_5, width / 2 - 50, height / 2 - 15, 120, 447); // Move image to the center
        }
        else if (currentDisplayedSymbolID == 6 && ID_6 != null)
        {
            g.DrawImage(ID_6, width / 2 - 50, height / 2-15, 120, 447); // Move image to the center
        }
        else if (currentDisplayedSymbolID == 7 && ID_7 != null)
        {
            g.DrawImage(ID_7, width / 2 - 50, height / 2 - 15, 120, 447); // Move image to the center
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
