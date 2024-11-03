using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using TUIO;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    int byteCT;
    public NetworkStream stream;
    byte[] sendData;
    public TcpClient client;

    public bool connectToSocket(string host, int portNumber)
    {
        try
        {
            client = new TcpClient(host, portNumber);
            stream = client.GetStream();
            Console.WriteLine("connection made ! with " + host);
            return true;
        }
        catch (System.Net.Sockets.SocketException e)
        {
            Console.WriteLine("Connection Failed: " + e.Message);
            return false;
        }
    }

    public string recieveMessage()
    {
        try
        {

            byte[] receiveBuffer = new byte[1024];
            int bytesReceived = stream.Read(receiveBuffer, 0, 1024);
            //Console.WriteLine(bytesReceived);
            string data = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
            //Console.WriteLine(data);
            return data;
        }
        catch (System.Exception e)
        {

        }

        return null;
    }

}

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
  
  
   
  
   
    //outfit groups


    //1
    private Image ID_2;
    private Image ID_28;
    private Image ID_29;

    //2
    private Image ID_3;
    private Image ID_33;
    private Image ID_34;
    //3
    private Image ID_4;
    private Image ID_44;
    private Image ID_45;
    //4
    private Image ID_5;
    private Image ID_55;
    private Image ID_56;
    //5
    private Image ID_6;
    private Image ID_66;
    private Image ID_67;
    //6
    private Image ID_7;
    private Image ID_77;
    private Image ID_78;





    // Display flag
    private int? currentDisplayedSymbolID = null; // Current SymbolID being displayed
    private int? currentSizeID = null;

    // Fixed positions for images
    private Point fixedPositionZero; // Initial position for SymbolID == 0 (left)
    private Point fixedPositionOne;  // Initial position for SymbolID == 1 (right)

    // Rectangle dimensions for SymbolID == 20-25 ("Rate This Outfit" and others)
    private Rectangle rateOutfitBox = new Rectangle(200, 300, 50, 150); // Slightly adjusted box size

    // Opacity and Zoom
    private bool useOpacity = false;
    private bool useZoom = false;
    private float opacity = 0.5f;
    private float alpha = 0;
    private float previousAlpha = 0;
    private int zoomControl = 0;

    public void stream()
    {
        Client c = new Client();
        c.connectToSocket("localhost", 4000);
        string msg = "";
        while (true)
        {
            msg = c.recieveMessage();
            Console.WriteLine(msg);
            if (msg == "exit")
            {
                c.stream.Close();
                c.client.Close();
                Console.WriteLine("Connection Terminated !");
                break;
            }
        }
    }

    public TuioDemo(int port)
    {

        Thread thread = new Thread(new ThreadStart(stream)); // create new thread to prevent blocking the form
        thread.IsBackground = true; // Ensures thread will close when the form closes
        thread.Start();
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
        {// Assigning images for each SymbolID with their size variants

            ID_2 = Image.FromFile("outm-1.PNG"); // Path for SymbolID 2s
            ID_28 = Image.FromFile("outm-1.PNG"); // Path for SymbolID 2m
            ID_29 = Image.FromFile("outm-1.PNG"); // Path for SymbolID 2l

            ID_3 = Image.FromFile("outm-2.PNG"); // Path for SymbolID 3s
            ID_33 = Image.FromFile("outm-2.PNG"); // Path for SymbolID 3m
            ID_34 = Image.FromFile("outm-2.PNG"); // Path for SymbolID 3l

            ID_4 = Image.FromFile("outm-3.PNG"); // Path for SymbolID 4s
            ID_44 = Image.FromFile("outm-3.PNG"); // Path for SymbolID 4m
            ID_45 = Image.FromFile("outm-3.PNG"); // Path for SymbolID 4l

            ID_5 = Image.FromFile("outf-1.PNG"); // Path for SymbolID 5s
            ID_55 = Image.FromFile("outf-1.PNG"); // Path for SymbolID 5m
            ID_56 = Image.FromFile("outf-1.PNG"); // Path for SymbolID 5l

            ID_6 = Image.FromFile("outf-2.PNG"); // Path for SymbolID 6s
            ID_66 = Image.FromFile("outf-2.PNG"); // Path for SymbolID 6m
            ID_67 = Image.FromFile("outf-2.PNG"); // Path for SymbolID 6l

            ID_7 = Image.FromFile("outf-3.PNG"); // Path for SymbolID 7s
            ID_77 = Image.FromFile("outf-3.PNG"); // Path for SymbolID 7m
            ID_78 = Image.FromFile("outf-3.PNG"); // Path for SymbolID 7l

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
        lock (objectList)
        {
            objectList.Add(o.SessionID, o);
        }
        if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);

        // Reset the displayed symbol ID if needed to avoid overlap
        bool shouldRedraw = false;

        if (o.SymbolID == 0 && ID_0 != null)
        {
            if (currentDisplayedSymbolID != 0)
            {
                currentDisplayedSymbolID = 0;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 1 && ID_1 != null)
        {
            if (currentDisplayedSymbolID != 1)
            {
                currentDisplayedSymbolID = 1;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID >= 20 && o.SymbolID <= 25) // For outfit symbols
        {
            if (currentDisplayedSymbolID != o.SymbolID)
            {
                currentDisplayedSymbolID = (int)o.SymbolID;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 2 && ID_2 != null)
        {
            if (currentDisplayedSymbolID != 2)
            {
                currentDisplayedSymbolID = 2;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 3 && ID_3 != null)
        {
            if (currentDisplayedSymbolID != 3)
            {
                currentDisplayedSymbolID = 3;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 4 && ID_4 != null)
        {
            if (currentDisplayedSymbolID != 4)
            {
                currentDisplayedSymbolID = 4;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 5 && ID_5 != null)
        {
            if (currentDisplayedSymbolID != 5)
            {
                currentDisplayedSymbolID = 5;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 6 && ID_6 != null)
        {
            if (currentDisplayedSymbolID != 6)
            {
                currentDisplayedSymbolID = 6;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 7 && ID_7 != null)
        {
            if (currentDisplayedSymbolID != 7)
            {
                currentDisplayedSymbolID = 7;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 28 && ID_28 != null)
        {
            if (currentDisplayedSymbolID != 28)
            {
                currentDisplayedSymbolID = 28;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 29 && ID_29 != null)
        {
            if (currentDisplayedSymbolID != 29)
            {
                currentDisplayedSymbolID = 29;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 33 && ID_33 != null)
        {
            if (currentDisplayedSymbolID != 33)
            {
                currentDisplayedSymbolID = 33;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 34 && ID_34 != null)
        {
            if (currentDisplayedSymbolID != 34)
            {
                currentDisplayedSymbolID = 34;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 44 && ID_44 != null)
        {
            if (currentDisplayedSymbolID != 44)
            {
                currentDisplayedSymbolID = 44;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 45 && ID_45 != null)
        {
            if (currentDisplayedSymbolID != 45)
            {
                currentDisplayedSymbolID = 45;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 55 && ID_55 != null)
        {
            if (currentDisplayedSymbolID != 55)
            {
                currentDisplayedSymbolID = 55;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 56 && ID_56 != null)
        {
            if (currentDisplayedSymbolID != 56)
            {
                currentDisplayedSymbolID = 56;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 66 && ID_66 != null)
        {
            if (currentDisplayedSymbolID != 66)
            {
                currentDisplayedSymbolID = 66;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 67 && ID_67 != null)
        {
            if (currentDisplayedSymbolID != 67)
            {
                currentDisplayedSymbolID = 67;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 77 && ID_77 != null)
        {
            if (currentDisplayedSymbolID != 77)
            {
                currentDisplayedSymbolID = 77;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 78 && ID_78 != null)
        {
            if (currentDisplayedSymbolID != 78)
            {
                currentDisplayedSymbolID = 78;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 100)
        {
            useOpacity = true;
            Invalidate();
        }
        else if (o.SymbolID == 101)
        {
            useZoom = true;
            Invalidate();
        }

        // Check for size changes
        if (o.SymbolID == 11)
        {
            if (currentSizeID != 11)
            {
                currentSizeID = 11;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 12)
        {
            if (currentSizeID != 12)
            {
                currentSizeID = 12;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 13)
        {
            if (currentSizeID != 13)
            {
                currentSizeID = 13;
                shouldRedraw = true;
            }
        }
        else if (o.SymbolID == 14)
        {
            if (currentSizeID != 14)
            {
                currentSizeID = 14;
                shouldRedraw = true;
            }
        }
        

        // Only trigger Invalidate if there was a change
        if (shouldRedraw)
        {
            Invalidate();
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

        // Reset flags if SymbolID 50 or 51 is removed
        if (o.SymbolID == 100)
        {
            useOpacity = false;
            opacity = 0.5f;
            alpha = 0;
            previousAlpha = 0;
        }
        else if (o.SymbolID == 101)
        {
            useZoom = false;
            zoomControl = 0;
        }

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
        

        // Background fill
        g.Clear(Color.FromArgb(189, 217, 229));
        DrawBackgroundImage(g);
        
        // Display the selected outfit rating box if an outfit is selected (SymbolID 20-25)
        // Clear previous images and only show the relevant outfit rating box if an outfit is selected (SymbolID 20-25)
        if (currentDisplayedSymbolID >= 20 && currentDisplayedSymbolID <= 25)
        {
            // Clear other selections
            DrawOutfitRatingBox(g);      // Draw only the outfit rating box
        }
        else if (currentSizeID == null && currentDisplayedSymbolID == null)
        {
            // Draw initial male and female images if no outfit or size is selected
            DrawInitialMaleAndFemaleImages(g);
        }
        else if (currentDisplayedSymbolID.HasValue)
        {
            // Draw only the current symbol (e.g., gender image) if no outfit is selected
            
            
                DrawCurrentSymbol(g);
            
        }
        if (currentSizeID != null)
        {
            Console.WriteLine($"Drawing size selection text for currentSizeID: {currentSizeID}");
            DrawSizeSelection(g);
        }

        // Only draw additional symbol images if no outfit rating box is displayed
        if (currentDisplayedSymbolID < 20 || currentDisplayedSymbolID > 25)
        {
            DrawSymbolIDImages(g);  // Show only images based on currentDisplayedSymbolID
        }


    }




    // Helper function to draw background image or fallback color
    private void DrawBackgroundImage(Graphics g)
    {
        if (back != null)
            g.DrawImage(back, new Rectangle(0, 0, width, height));
        else
            g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));
    }



    // Draw male and female images if no size selected
    private void DrawInitialMaleAndFemaleImages(Graphics g)
    {
        DrawImageAtPosition(g, leftOfMale, fixedPositionZero.X - 150, fixedPositionZero.Y + 150, 170, 70);
        DrawImageAtPosition(g, ID_0, fixedPositionZero.X, fixedPositionZero.Y - 50, 120, 447);
        DrawImageAtPosition(g, rightOfFemale, fixedPositionOne.X + 95, fixedPositionOne.Y + 150, 170, 70);
        DrawImageAtPosition(g, ID_1, fixedPositionOne.X, fixedPositionOne.Y - 50, 120, 447);
    }


    // Draws the symbol and adjusts size and border based on currentSizeID
    private void DrawCurrentSymbol(Graphics g)
    {
        
        Image currentImage = currentDisplayedSymbolID == 0 ? ID_0 : ID_1;
        var dimensions = GetDimensionsForSize(currentSizeID);
        int figureWidth = dimensions.Item1;
        int figureHeight = dimensions.Item2;

        if (currentImage != null)
        {

            g.Clear(this.BackColor);
            if (back != null)
            {
                DrawBackgroundImage(g);
            }


            int centerX = width / 2 - figureWidth / 2;
            int centerY = height /2 - figureHeight /6;
            g.DrawImage(currentImage, centerX, centerY, figureWidth, figureHeight);
            

           
        }
    }

    // Draws size selection text
    private void DrawSizeSelection(Graphics g)
    {
        string sizeText;
        int size;
        switch (currentSizeID)
        {
            case 11:
                sizeText = "Selected Size: Small";
                size = 1;
                break;
            case 12:
                sizeText = "Selected Size: Medium";
                size = 2;
                break;
            case 13:
                sizeText = "Selected Size: Large";
                size = 3;
                break;
            case 14:
                sizeText = "Selected Size: X-Large";
                size = 4;
                break;
            default:
                sizeText = "Selected Size: Default";
                break;
        }


       // g.DrawString(sizeText, new Font("Georgia", 24), Brushes.Black, new PointF(50, 50));
    }

    // Draws the outfit rating box if the SymbolID is between 20 and 25
    private void DrawOutfitRatingBox(Graphics g)
    {
        Image outfitImage = GetOutfitImage(currentDisplayedSymbolID);
        if (outfitImage != null)
        {

            g.Clear(this.BackColor);
            if (back != null)
            {
                DrawBackgroundImage(g);
            }

            int newWidth = outfitImage.Width / 2;
            int newHeight = outfitImage.Height / 2;
            int centerX = (width / 2) - (newWidth / 2);
            int centerY = (height / 2) - (newHeight / 2);

            g.DrawImage(outfitImage, centerX, centerY, newWidth, newHeight);
            g.DrawString("Rate This Outfit", new Font("Arial", 16), Brushes.Black, new Point(centerX + 100, centerY - 30));
        }
    }




    // Display images for SymbolIDs 2-7
    // Define a class to hold symbol image information
    public class SymbolImageInfo
    {
        public Image Image { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public SymbolImageInfo(Image image, int width, int height)
        {
            Image = image;
            Width = width;
            Height = height;
        }
    }

    // Display images for specified SymbolIDs with associated sizes
    private void DrawSymbolIDImages(Graphics g)
    {
        // Define images and sizes for each SymbolID
        var symbolImageData = new Dictionary<int, SymbolImageInfo>
    {
            //outfit1
    { 2, new SymbolImageInfo(ID_2, 100, 400) },   // Small
    { 28, new SymbolImageInfo(ID_28, 130, 450) }, // Medium
    { 29, new SymbolImageInfo(ID_29, 160, 500) }, // Large
    
    //outfit2
    { 3, new SymbolImageInfo(ID_3, 100, 400) },    // Small
    { 33, new SymbolImageInfo(ID_33, 130, 450) },  // Medium
    { 34, new SymbolImageInfo(ID_34, 160, 500) },  // Large
    //outfit3
    
    { 4, new SymbolImageInfo(ID_4, 100, 400) },    // Small
    { 44, new SymbolImageInfo(ID_44, 130, 450) },  // Medium
    { 45, new SymbolImageInfo(ID_45, 160, 500) },  // Large
    
    //outfit4
    { 5, new SymbolImageInfo(ID_5, 100, 400) },    // Small
    { 55, new SymbolImageInfo(ID_55, 130, 450) },  // Medium
    { 56, new SymbolImageInfo(ID_56, 160, 500) },  // Large
    //outfit5
    { 6, new SymbolImageInfo(ID_6, 100, 400) },    // Small
    { 66, new SymbolImageInfo(ID_66, 130, 450) },  // Medium
    { 67, new SymbolImageInfo(ID_67, 160, 500) },  // Large
    //outfit6
    { 7, new SymbolImageInfo(ID_7, 100, 400) },    // Small
    { 77, new SymbolImageInfo(ID_77, 130, 450) },  // Medium
    { 78, new SymbolImageInfo(ID_78, 160, 500) }   // Large
    };

        // Check if currentDisplayedSymbolID has an entry in symbolImageData
        if (currentDisplayedSymbolID.HasValue &&
            symbolImageData.TryGetValue(currentDisplayedSymbolID.Value, out SymbolImageInfo symbolInfo))
        {
            // Extract the symbol image, width, and height from the symbolInfo object
            var symbolImage = symbolInfo.Image;
            var newWidth = symbolInfo.Width;
            var newHeight = symbolInfo.Height;

            // Clear previous drawings and draw the background
            g.Clear(this.BackColor);
            if (back != null)
            {
                DrawBackgroundImage(g);
            }

            // Calculate center position
            int centerX = (width / 2) - (newWidth / 2);
            int centerY = (height ) - (newHeight );

            // Draw the symbol image with specified size and position
            if (useOpacity == true || useZoom == true)
            {
                lock (blobList)
                {
                    if (objectList.Count > 0)
                    {
                        foreach (TuioObject tobj in objectList.Values.ToList())
                        {

                            alpha = (float)(tobj.Angle / Math.PI * 180.0f);

                            if (alpha > 180) // this if statement is created because tobj.angle has only +ve values
                            {
                                alpha -= 360; // Adjust to the negative range
                            }

                            if (useOpacity == true)
                            {
                                // Calculate opacity based on angle
                                if (alpha >= 0)
                                {
                                    if (alpha > previousAlpha)
                                    {
                                        if (opacity < 1.0f)
                                        {
                                            opacity += 0.1f;
                                            //Console.WriteLine(opacity);
                                        }
                                        previousAlpha = alpha;
                                    }
                                    else if (alpha < previousAlpha)
                                    {
                                        if (opacity > 0.3f)
                                        {
                                            opacity -= 0.1f;
                                            //Console.WriteLine(opacity);
                                        }
                                        previousAlpha = alpha;
                                    }
                                }

                                Console.WriteLine($"Alpha = {alpha}| Previous Alpha = {previousAlpha} | Opacity = {opacity}");

                                // Create a ColorMatrix and set its opacity
                                ColorMatrix colorMatrix = new ColorMatrix();
                                colorMatrix.Matrix33 = opacity;  // Matrix33 represents the alpha value (opacity)

                                // Create ImageAttributes to apply the ColorMatrix
                                ImageAttributes imageAttributes = new ImageAttributes();
                                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                                g.DrawImage(symbolImage, new Rectangle(centerX, centerY, newWidth + zoomControl, newHeight + zoomControl),
                                            0, 0, symbolImage.Width, symbolImage.Height, GraphicsUnit.Pixel, imageAttributes);
                
                            }
                            else if (useZoom == true)
                            {
                                // Calculate Zoom based on angle
                                if (alpha < 0)
                                {
                                    if (alpha < previousAlpha)
                                    {
                                        if (zoomControl < 30)
                                        {
                                            zoomControl += 2;
                                        }
                                        previousAlpha = alpha;
                                    }
                                    else if (alpha > previousAlpha)
                                    {
                                        if (zoomControl > 0)
                                        {
                                            zoomControl -= 2;
                                        }
                                        previousAlpha = alpha;
                                    }
                                }

                                Console.WriteLine($"Zoom Scale = {zoomControl}");
                                
                                g.DrawImage(symbolImage, new Rectangle(centerX, centerY, newWidth + zoomControl, newHeight + zoomControl));
                            }
                        }

                    }
                }
            }
            else
            {
                g.DrawImage(symbolImage, centerX, centerY, newWidth, newHeight);
            }
            
        }
    }


    // Helper method to draw an image at a specified position
    private void DrawImageAtPosition(Graphics g, Image img, int x, int y, int width, int height)
    {
        if (img != null)
        {
            g.DrawImage(img, x, y, width, height);
        }
    }

    // Helper functions for dynamic sizing and border color
    private Tuple<int, int> GetDimensionsForSize(int? sizeID)
    {
        switch (sizeID)
        {
            case 11: return Tuple.Create(100, 425);
            case 12: return Tuple.Create(130, 450);
            case 13: return Tuple.Create(160, 475);
            case 14: return Tuple.Create(200, 500);
            default: return Tuple.Create(120, 450);
        }
    }

    private Color GetBorderColor(int? sizeID)
    {
        switch (sizeID)
        {
            case 11: return Color.Blue;
            case 12: return Color.Green;
            case 13: return Color.Orange;
            case 14: return Color.Red;
            default: return Color.DarkGray;
        }
    }

    private int GetBorderWidth(int? sizeID)
    {
        switch (sizeID)
        {
            case 11: return 2;
            case 12: return 4;
            case 13: return 6;
            case 14: return 8;
            default: return 4;
        }
    }

    // Retrieve outfit image based on SymbolID
    private Image GetOutfitImage(int? symbolID)
    {
        switch (symbolID)
        {
            case 20: return rateOutfitImage;
            case 21: return rateOutfitImage21;
            case 22: return rateOutfitImage22;
            case 23: return rateOutfitImage23;
            case 24: return rateOutfitImage24;
            case 25: return rateOutfitImage25;
            default: return null;
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
