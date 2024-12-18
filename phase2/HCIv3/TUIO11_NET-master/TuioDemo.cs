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
using System.IO.Ports;

public class Client
{
    public NetworkStream stream;
    public TcpClient client;

    public bool connectToSocket(string host, int portNumber)
    {
        try
        {
            client = new TcpClient(host, portNumber);
            stream = client.GetStream();
            Console.WriteLine("Connection made with " + host);
            return true;
        }
        catch (SocketException e)
        {
            Console.WriteLine("Connection Failed: " + e.Message);
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("General Error: " + e.Message);
            return false;
        }
    }

    public string receiveMessage()
    {
        try
        {
            if (stream == null)
            {
                Console.WriteLine("Stream is not initialized.");
                return null;
            }

            byte[] receiveBuffer = new byte[1024];
            int bytesReceived = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
            return Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error receiving message: " + e.Message);
            return null;
        }
    }

    public void streamMessages()
    {
        while (true)
        {
            string message = receiveMessage();
            if (message == "exit")
            {
                Console.WriteLine("Connection Terminated!");
                break;
            }
            Console.WriteLine("Received: " + message);
        }

        stream?.Close();
        client?.Close();
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

    // Images
    private Image back_m = null; // Background image for the form
    private Image back_f = null;
    private Image back1; // Initial background image to appear first

    private Image ID_0s;
    private Image ID_0m;// Image for SymbolID == 0 (Male)
    private Image ID_0l;

    private Image ID_1s;  
    private Image ID_1m;// Image for SymbolID == 1 (Female)
    private Image ID_1l;

    private Image leftOfMale; // New image to the left of the male character
    private Image rightOfFemale; // New image to the right of the female character

    // New Images for SymbolIDs 20-25 ("Rate This Outfit")
    private Image rate;
    private Image rate1;
    private Image rate2;
    private Image rate3;
    private Image rate4;
    private Image rate5;


    //choose size image
    private Image size;

    //male outfits
    private Image ID_01s;

    private Image ID_03s;
                     
    private Image ID_01m;
   
    private Image ID_03m;
                     
    private Image ID_01l;
 
    private Image ID_03l;

    private Image outm;


    //female outfits
    private Image ID_11s;
    private Image ID_12s; //size small
    
                     
    private Image ID_11m;
    private Image ID_12m; //size meduim
   
                     
    private Image ID_11l;
    private Image ID_12l; //size large
    
    private Image outf;




    ////buttons
    private Image next;
    private Image pre;
    private Image cho;
    private Image chg;
    private Image chs;
    private Image chr;


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
    private int inside = -1;
    private bool DrawMenu = false;
    private int counter = 0;
    private int flag = -1;
    private string gender = null;
    private string GestureMsg = null;
    private string ObjectMsg = null;
    private string BluetoothMsg = null;
    private bool connToBluetooth = true;

    /////// MARINA FIX SAD / BLUETOOTH

    public void stream(string ip, int portNumber)
    {
        Client c = new Client();
        if (c.connectToSocket(ip, portNumber)) // Consistent port
        {
            string msg = "";
            while (true)
            {
                msg = c.receiveMessage();

                if (msg != null)
                {
                    //Console.WriteLine(msg);

                    if (msg == "vest_dress")
                    {
                        ObjectMsg = msg;
                        Console.WriteLine(ObjectMsg);
                    }
                    
                    if (msg == "F")
                    {
                        Console.WriteLine(msg);
                        flag = 1;
                        currentDisplayedSymbolID = 1;
                        //Console.WriteLine(flag);
                        connToBluetooth = false;
                        BluetoothMsg = msg;
                    }
                    else if (msg == "M")
                    {
                        Console.WriteLine(msg);
                        flag = 0;
                        currentDisplayedSymbolID = 0;
                        //Console.WriteLine(flag);
                        connToBluetooth = false;
                        BluetoothMsg = msg;

                    }
                    

                    if(ip == "127.0.0.1")
                    {
                        GestureMsg = msg;
                        Console.WriteLine(msg);
                    }
                   
                    
                   /* if (msg == "Marena Anis - Happy")
                    {
                        Console.WriteLine("yahhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh");
                        //flag = 1;
                        //currentDisplayedSymbolID = 1;
                        //Console.WriteLine(flag);
                    }*/
                    
                    if (msg == "exit")
                    {
                        c.stream?.Close();
                        c.client?.Close();
                        Console.WriteLine("Connection Terminated!");
                        break;
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Could not connect to the server.");
        }
    }

   

    public TuioDemo(int port)
    {

        /*Thread handGestureThread = new Thread(() => stream("127.0.0.1", 4000));
        handGestureThread.IsBackground = true; // Ensures thread will close when the form closes
        handGestureThread.Start();*/

        Thread objectDetectionThread = new Thread(() => stream("127.0.0.1", 4000));// create new thread to prevent blocking the form
        objectDetectionThread.IsBackground = true; // Ensures thread will close when the form closes
        objectDetectionThread.Start();

        /*Thread faceThread = new Thread(() => stream("127.0.0.3", 6000));// create new thread to prevent blocking the form
        faceThread.IsBackground = true; // Ensures thread will close when the form closes
        faceThread.Start();*/

        Thread blueThread = new Thread(() => stream("127.0.0.4", 7000));// create new thread to prevent blocking the form
        blueThread.IsBackground = true; // Ensures thread will close when the form closes
        blueThread.Start();

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
            back1 = Image.FromFile("back1.PNG"); // intro
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading initial background image: " + ex.Message);
            back1 = null; // Ensure it's null if loading fails
        }
        /*try
        {
            

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading background image: " + ex.Message);
            back_m = null;
            back_f = null;// Ensure it's null if loading fails
        }*/
        


        // Load the images for SymbolID 0 (Male) and 1 (Female)
        try
        {
            ID_0s = Image.FromFile("mm-1.PNG"); 
            ID_0m = Image.FromFile("mm-1.PNG"); 
            ID_0l = Image.FromFile("mm-1.PNG"); 
                                        
            ID_1s = Image.FromFile("ff-3.PNG");
            ID_1m = Image.FromFile("ff-3.PNG"); 
            ID_1l = Image.FromFile("ff-3.PNG"); 
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading image: " + ex.Message);
        }

        // Load the images for outfits
        try
        {
            //male outfits
            ID_01s = Image.FromFile("mm-3.PNG");                         
            ID_01m = Image.FromFile("mm-3.PNG");                            
            ID_01l = Image.FromFile("mm-3.PNG");

            ID_03s = Image.FromFile("mm-2.PNG");
            ID_03m = Image.FromFile("mm-2.PNG");
            ID_03l = Image.FromFile("mm-2.PNG");


            //female outfits
            ID_11s = Image.FromFile("ff-1.PNG");
            ID_12s = Image.FromFile("ff-2.PNG");
           
                                     
            ID_11m = Image.FromFile("ff-1.PNG");
            ID_12m = Image.FromFile("ff-2.PNG");
            
                                     
            ID_11l = Image.FromFile("ff-1.PNG");
            ID_12l = Image.FromFile("ff-2.PNG");
          


        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading image: " + ex.Message);
        }
    

        // Set fixed positions for male and female images (left and right of the screen)
        fixedPositionZero = new Point(600, height / 2-5); // Position for male image on the left
        fixedPositionOne = new Point(800, height / 2-5);  // Position for female image on the right
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

    private bool showBack1 = true;
    private object g;

    public void addTuioObject(TuioObject o)
 {
        lock (objectList)
        {
            objectList.Add(o.SessionID, o);
        }
        if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);

        bool shouldRedraw = false;

        // Handle special SymbolID 200 to remove back1 image
        if (o.SymbolID == 200)
        {
            showBack1 = false;
            shouldRedraw = true;
            inside = -1;
        }
        // Display specific images for male and female based on SymbolID and size
        else if (o.SymbolID == 0 && ID_0m != null) // Male, medium
        {
            currentDisplayedSymbolID = 0;
            currentSizeID = null; // Reset size selection
            shouldRedraw = true;
            gender = "male";
            inside = 0;
        }
        else if (o.SymbolID == 2 && ID_0s != null) // Male, small
        {
            currentDisplayedSymbolID = 2;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 2;
        }
        else if (o.SymbolID == 4 && ID_0l != null) // Male, large
        {
            currentDisplayedSymbolID = 4;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 4;
        }
        /////small sizes for the 3 male outfits
        else if (o.SymbolID == 6 && ID_01s != null)
        {
            currentDisplayedSymbolID = 6;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 6;
        }
        else if (o.SymbolID == 12 && ID_03s != null)
        {
            currentDisplayedSymbolID = 12;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 12;
        }

        /////meduim sizes for the 3 male outfits
        else if (o.SymbolID == 7 && ID_01m != null)
        {
            currentDisplayedSymbolID = 7;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 7;
        }
       
        else if (o.SymbolID == 13 && ID_03m != null)
        {
            currentDisplayedSymbolID = 13;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 13;
        }

        /////large sizes for the 3 male outfits
        else if (o.SymbolID == 8 && ID_01l != null)
        {
            currentDisplayedSymbolID = 8;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 8;
        }
      
        else if (o.SymbolID == 14 && ID_03l != null)
        {
            currentDisplayedSymbolID = 14;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 14;
        }

        else if (o.SymbolID == 1 && ID_1m != null) // Female, medium
        {
            currentDisplayedSymbolID = 1;
            currentSizeID = null;
            shouldRedraw = true;
            gender = "female";
            inside = 1;
        }
        else if (o.SymbolID == 3 && ID_1s != null) // Female, small
        {
            currentDisplayedSymbolID = 3;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 3;
        }
        else if (o.SymbolID == 5 && ID_1l != null) // Female, large
        {
            currentDisplayedSymbolID = 5;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 5;
        }

        /////small sizes for the 3 female outfits
        else if (o.SymbolID == 15 && ID_11s != null)
        {
            currentDisplayedSymbolID = 15;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 15;
        }
        else if (o.SymbolID == 18 && ID_12s != null)
        {
            currentDisplayedSymbolID = 18;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 18;
        }


        /////meduim sizes for the 3 female outfits
        else if (o.SymbolID == 16 && ID_11m != null)
        {
            currentDisplayedSymbolID = 16;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 16;
        }
        else if (o.SymbolID == 19 && ID_12m != null)
        {
            currentDisplayedSymbolID = 19;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 19;
        }


        /////large sizes for the 3 female outfits
        else if (o.SymbolID == 17 && ID_11l != null)
        {
            currentDisplayedSymbolID = 17;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 17;
        }
        else if (o.SymbolID == 20 && ID_12l != null)
        {
            currentDisplayedSymbolID = 20;
            currentSizeID = null;
            shouldRedraw = true;
            inside = 20;
        }



        // Show the size image when SymbolID 201 is detected
        else if (o.SymbolID == 201 && size != null)
        {
            currentSizeID = 201;
            currentDisplayedSymbolID = null; // Clear other images when showing size
            shouldRedraw = true;
            inside = -1;
        }
        else if (o.SymbolID == 202 && outm != null)
        {
            currentSizeID = 202;
            currentDisplayedSymbolID = null; // Clear other images when showing size
            shouldRedraw = true;
            inside = -1;
        }
        else if (o.SymbolID == 203 && outf != null)
        {
            currentSizeID = 203;
            currentDisplayedSymbolID = null; // Clear other images when showing size
            shouldRedraw = true;
            inside = -1;
        }
        // Handle "Rate This Outfit" images with SymbolIDs 20-25
        else if (o.SymbolID >= 30 && o.SymbolID <= 35)
        {
            currentDisplayedSymbolID = (int)o.SymbolID;
            currentSizeID = null;
            shouldRedraw = true;
            inside = -1;
        }
        // Opacity and zoom controls
        else if (o.SymbolID == 100)
        {
            if (ObjectMsg == null)
            {
                DrawMenu = true;
            }     
            currentDisplayedSymbolID = null;
            //useOpacity = true;
            Invalidate();
            inside = 100;
        }
        else if (o.SymbolID == 101)
        {
            useZoom = true;
            Invalidate();
            inside = 101;
        }

        // Trigger repaint if needed
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
            if (ObjectMsg == null)
            {
                DrawMenu = false;
            }
            counter = 0;
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
        //base.OnPaintBackground(pevent);
        Graphics g = pevent.Graphics;

        // Clear the background
        g.Clear(Color.FromArgb(189, 217, 229));

        /////////////////////////////////////////////////////////DRAW BLUETOOTH///////////////////////////////////////////////////////////////////////////////////////////////
        if (BluetoothMsg == "M")
        {
            Console.WriteLine("in");
            //currentDisplayedSymbolID = 0;
            gender = "male";
            back_m = Image.FromFile("back-m.PNG");

            DrawBLUETOOTH(g);
        }
        else if (BluetoothMsg == "F")
        {
            // MessageBox.Show("a3333333333333333333333");
            //currentDisplayedSymbolID = 1;
            gender = "female";
            back_f = Image.FromFile("back-f.PNG");
            DrawBLUETOOTH(g);
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        if (ObjectMsg == "vest_dress" && gender == "femle")
        {
            Console.WriteLine("hii222");
            DrawCircularMenu(g);
        }
        else if(ObjectMsg == "short_sleeved_shirt" && gender == "male")
        {
            DrawCircularMenu(g);
        }

        Console.WriteLine(currentDisplayedSymbolID);

        // Draw back1 initially if it exists; otherwise, draw the main background image (back) or fill color
        if (showBack1 && back1 != null && DrawMenu == false && back_f == null && back_m == null)
        {
            g.DrawImage(back1, new Rectangle(0, 0, width, height));
        }
        else
        {
            DrawBackgroundImage(g); // Fallback to the default background
        }

       
        if (DrawMenu == true && gender != null)
        {
            //g.Clear(Color.FromArgb(189, 217, 229));
            //Console.WriteLine("DrawMenu On");
            DrawCircularMenu(g);
        }

        

        // Prioritize displaying the size image if currentSizeID is 201
        if (currentSizeID == 201 && size != null)
        {
            int centerX = width / 2 - size.Width / 2;
            int centerY = height / 2 - size.Height / 2;
            g.DrawImage(size, centerX, centerY, size.Width, size.Height);
            return; // Return early to prevent other images from being drawn over
        }
        if (currentSizeID == 202 && outm != null)
        {
            int centerX = width / 2 - size.Width / 2;
            int centerY = height / 2 - size.Height / 2;
            g.DrawImage(outm, centerX, centerY, size.Width, size.Height);
            return; // Return early to prevent other images from being drawn over
        }
        if (currentSizeID == 203 && outf != null)
        {
            int centerX = width / 2 - size.Width / 2;
            int centerY = height / 2 - size.Height / 2;
            g.DrawImage(outf, centerX, centerY, size.Width, size.Height);
            return; // Return early to prevent other images from being drawn over
        }

        if (currentDisplayedSymbolID.HasValue && DrawMenu == false)
        {
            DrawCurrentSymbol(g);
        }
        // If size image is not active, display other images based on current selections
        if (!showBack1)
        {
            if (currentSizeID == null && currentDisplayedSymbolID == null)
            {
                DrawInitialMaleAndFemaleImages(g);
            }
            else if (currentDisplayedSymbolID.HasValue)
            {
                DrawCurrentSymbol(g);
            }
        }
        // Only draw additional symbol images if no outfit rating box is displayed
        if (currentDisplayedSymbolID < 20 || currentDisplayedSymbolID > 25)
        {
            DrawSymbolIDImages(g);  // Show only images based on currentDisplayedSymbolID
        }
    }

////opacity & zoom
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

    private void DrawSymbolIDImages(Graphics g)
    {
        // Define images and sizes for each SymbolID
        var symbolImageData = new Dictionary<int, SymbolImageInfo>
        {
            //male outfit1 
            { 6, new SymbolImageInfo(ID_01s, 155, 500) },   // Small
            { 7, new SymbolImageInfo(ID_01m, 170, 500) }, // Medium
            { 8, new SymbolImageInfo(ID_01l, 200, 500) }, // Large

            //male outfit2 
            { 9, new SymbolImageInfo(ID_01s, 155, 500) },   // Small
            { 10, new SymbolImageInfo(ID_01m, 170, 500) }, // Medium
            { 11, new SymbolImageInfo(ID_01l, 200, 500) }, // Large
    
            //male outfit3
            { 12, new SymbolImageInfo(ID_03s, 155, 500) },    // Small
            { 13, new SymbolImageInfo(ID_03m, 170, 500) },  // Medium
            { 14, new SymbolImageInfo(ID_03l, 200, 500) },  // Large
    
            //female outfit1
            { 15, new SymbolImageInfo(ID_11s, 155, 500) },    // Small
            { 16, new SymbolImageInfo(ID_11m, 170, 500) },  // Medium
            { 17, new SymbolImageInfo(ID_11l, 200, 500) },  // Large
            //female outfit2
            { 18, new SymbolImageInfo(ID_12s, 155, 500) },    // Small
            { 19, new SymbolImageInfo(ID_12m, 170, 500) },  // Medium
            { 20, new SymbolImageInfo(ID_12l, 200, 500) },  // Large
   
        };
        // Check if currentDisplayedSymbolID has an entry in symbolImageData
        if (currentDisplayedSymbolID.HasValue &&
            symbolImageData.TryGetValue(currentDisplayedSymbolID.Value, out SymbolImageInfo symbolInfo) && DrawMenu == false)
        {
            // Extract the symbol image, width, and height from the symbolInfo object
            var symbolImage = symbolInfo.Image;
            var newWidth = symbolInfo.Width;
            var newHeight = symbolInfo.Height;

            // Clear previous drawings and draw the background
            g.Clear(this.BackColor);
            if (back_m != null || back_f != null)
            {
                DrawBackgroundImage(g);
            }

            // Calculate center position
            int centerX = (width / 2) - (newWidth / 2);
            int centerY = (height) - (newHeight);

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

    
    //  draw background 
    private void DrawBackgroundImage(Graphics g)
    {
        if (back_m != null && gender == "male")
        {
            g.DrawImage(back_m, new Rectangle(0, 0, width, height));
        }
        else if (back_f != null && gender == "female")
        {
            g.DrawImage(back_f, new Rectangle(0, 0, width, height));
        }
         else
            g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));
    }

    // Draw male and female images if no size selected
    private void DrawInitialMaleAndFemaleImages(Graphics g)
    {
        //female and male signs button
        //DrawImageAtPosition(g, leftOfMale, fixedPositionZero.X - 150, fixedPositionZero.Y + 150, 170, 70);
        //DrawImageAtPosition(g, rightOfFemale, fixedPositionOne.X + 140, fixedPositionOne.Y + 150, 170, 70);
        //male=0
       // if(flag==0)
        DrawImageAtPosition(g, ID_0m, fixedPositionZero.X, fixedPositionZero.Y - 50, 180, 500);
        //female=1
        //if(flag==1)
        DrawImageAtPosition(g, ID_1m, fixedPositionOne.X, fixedPositionOne.Y - 50, 160, 500);

    }

    // Draws the symbol and adjusts size and border based on currentSizeID
    private void DrawCurrentSymbol(Graphics g)
    {
        // Determine the image, size, and position based on currentDisplayedSymbolID
        Image currentImage = null;
        int figureWidth = 180;  // Default width
        int figureHeight = 530; // Default height
        int centerX = 0;        // Default X position
        int centerY = 0;        // Default Y position

        // Adjust dimensions and positions for male outfits
        switch (currentDisplayedSymbolID)
        {
            // Male main character sizes
            case 0: // Medium
                currentImage = ID_0m;
                figureWidth = 180;
                figureHeight = 530;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 6;
                break;
            case 2: // Small
                currentImage = ID_0s;
                figureWidth = 155;
                figureHeight = 530;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 4 + 50;
                break;
            case 4: // Large
                currentImage = ID_0l;
                figureWidth = 230;
                figureHeight = 530;
                centerX = width / 2 - figureWidth / 2 + 10;
                centerY = height / 2 - figureHeight / 3 + 90;
                break;

            // Male outfit 1 (small, medium, large)
            case 6:  // Small
                currentImage = ID_01s;
                figureWidth = 155;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 4 + 60;
                break;
            case 7:  // Medium
                currentImage = ID_01m;
                figureWidth = 170;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 6 + 20;
                break;
            case 8:  // Large
                currentImage = ID_01l;
                figureWidth = 200;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 10;
                centerY = height / 2 - figureHeight / 3 + 100;
                break;

      

            // Male outfit 3 (small, medium, large)
            case 12:  // Small
                currentImage = ID_03s;
                figureWidth = 155;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 4 + 60;
                break;
            case 13:  // Medium
                currentImage = ID_03m;
                figureWidth = 170;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 6 + 20;
                break;
            case 14:  // Large
                currentImage = ID_03l;
                figureWidth = 200;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 10;
                centerY = height / 2 - figureHeight / 3 + 100;
                break;

            // Female main character sizes
            case 1: // Medium
                currentImage = ID_1m;
                figureWidth = 170;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 6 + 20;
                break;
            case 3: // Small
                currentImage = ID_1s;
                figureWidth = 155;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 4 + 60;
                break;
            case 5: // Large
                currentImage = ID_1l;
                figureWidth = 200;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 10;
                centerY = height / 2 - figureHeight / 3 + 100;
                break;

            // Female outfit 1 (small, medium, large)
            case 15: // Small
                currentImage = ID_11s;
                figureWidth = 155;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 4 + 60;
                break;
            case 16: // Medium
                currentImage = ID_11m;
                figureWidth = 170;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 6 + 20;
                break;
            case 17: // Large
                currentImage = ID_11l;
                figureWidth = 200;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 10;
                centerY = height / 2 - figureHeight / 3 + 100;
                break;

            // Female outfit 2 (small, medium, large)
            case 18: // Small
                currentImage = ID_12s;
                figureWidth = 155;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 4 + 60;
                break;
            case 19: // Medium
                currentImage = ID_12m;
                figureWidth = 170;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 6 + 20;
                break;
            case 20: // Large
                currentImage = ID_12l;
                figureWidth = 200;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 10;
                centerY = height / 2 - figureHeight / 3 + 100;
                break;

            case 30:
                currentImage = rate;
                figureWidth = 400;
                figureHeight = 150;
                centerX = (width / 2) - (figureWidth / 2);
                centerY = (height / 2) - (figureHeight / 2);
                break;
            case 31:
                currentImage = rate1;
                figureWidth = 400;
                figureHeight = 150;
                centerX = (width / 2) - (figureWidth / 2);
                centerY = (height / 2) - (figureHeight / 2);
                break;
            case 32:
                currentImage = rate2;
                figureWidth = 400;
                figureHeight = 150;
                centerX = (width / 2) - (figureWidth / 2);
                centerY = (height / 2) - (figureHeight / 2);
                break;
            case 33:
                currentImage = rate3;
                figureWidth = 400;
                figureHeight = 150;
                centerX = (width / 2) - (figureWidth / 2);
                centerY = (height / 2) - (figureHeight / 2);
                break;
            case 34:
                currentImage = rate4;
                figureWidth = 400;
                figureHeight = 150;
                centerX = (width / 2) - (figureWidth / 2);
                centerY = (height / 2) - (figureHeight / 2);
                break;
            case 35:
                currentImage = rate5;
                figureWidth = 400;
                figureHeight = 150;
                centerX = (width / 2) - (figureWidth / 2);
                centerY = (height / 2) - (figureHeight / 2);
                break;
        }

        if (currentImage != null)
        {
            g.Clear(this.BackColor);
            DrawBackgroundImage(g); // Draw background

            // Draw the selected image with the specified dimensions and position
            g.DrawImage(currentImage, centerX, centerY, figureWidth, figureHeight);
        }
    }

    private void DrawBLUETOOTH(Graphics g)
    {
        // Determine the image, size, and position based on currentDisplayedSymbolID
        Image currentImage = null;
        int figureWidth = 180;  // Default width
        int figureHeight = 530; // Default height
        int centerX = 0;        // Default X position
        int centerY = 0;        // Default Y position

        // Adjust dimensions and positions for male outfits
        switch (currentDisplayedSymbolID)
        {
            // Male main character sizes
            case 0: // Medium
                currentImage = ID_0m;
                figureWidth = 180;
                figureHeight = 530;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 6;
                break;
        
            case 1:  // Medium
                currentImage = ID_01m;
                figureWidth = 170;
                figureHeight = 500;
                centerX = width / 2 - figureWidth / 2 + 20;
                centerY = height / 2 - figureHeight / 6 + 20;
                break;
           
        }

        if (currentImage != null)
        {
            g.Clear(this.BackColor);
            DrawBackgroundImage(g); // Draw background

            // Draw the selected image with the specified dimensions and position
            g.DrawImage(currentImage, centerX, centerY, figureWidth, figureHeight);
        }
    }



    








    // to draw an image at a specified position
    private void DrawImageAtPosition(Graphics g, Image img, int x, int y, int width, int height)
    {
        if (img != null)
        {
            g.DrawImage(img, x, y, width, height);
        }
    }

    private void DrawCircularMenu(Graphics g)
    {
        currentDisplayedSymbolID = null;
        currentSizeID = null;
        //g.FillRectangle(bgrBrush, new Rectangle(0, 0, screen_width, screen_height));
        Rectangle rect1;
        Rectangle rect2;
        Rectangle rect3;
        if (gender == "male")
        {
            g.DrawImage(ID_0s, screen_width / 2 - 80, screen_height / 2 - 80, 100, 200);
            g.DrawImage(ID_03s, screen_width / 2 + (80), screen_height / 2, 100, 200);
            g.DrawImage(ID_01s, screen_width / 2 - (80 * 3), screen_height / 2, 100, 200);

            rect1 = new Rectangle(screen_width / 2 - 80, screen_height / 2 - 80, 100, 200);
            rect3 = new Rectangle(screen_width / 2 - (80 * 3), screen_height / 2, 100, 200);
            rect2 = new Rectangle(screen_width / 2 + (80), screen_height / 2, 100, 200);
        }
        else
        {
            g.DrawImage(ID_1s, screen_width / 2 - 80, screen_height / 2 - 80, 100, 200);
            g.DrawImage(ID_11s, screen_width / 2 + (80), screen_height / 2, 100, 200);
            g.DrawImage(ID_12s, screen_width / 2 - (80 * 3), screen_height / 2, 100, 200);

            rect1 = new Rectangle(screen_width / 2 - 80, screen_height / 2 - 80, 100, 200);
            rect3 = new Rectangle(screen_width / 2 - (80 * 3), screen_height / 2, 100, 200);
            rect2 = new Rectangle(screen_width / 2 + (80), screen_height / 2, 100, 200);
        }




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
                    if (GestureMsg == null)
                    {
                        if (gender == "male")
                        {
                            if (alpha > previousAlpha)
                            {

                                if (counter <= 2)
                                {
                                    counter++;
                                }
                                else
                                {
                                    counter = 0;
                                }

                                previousAlpha = alpha;
                            }
                            else if (alpha < previousAlpha)
                            {
                                if (counter > 0)
                                {
                                    counter--;
                                }
                                else
                                {
                                    counter = 2;
                                }
                                previousAlpha = alpha;
                            }
                        }
                        else
                        {
                            if (alpha > previousAlpha)
                            {

                                if (counter <= 2)
                                {
                                    counter++;
                                }
                                else
                                {
                                    counter = 0;
                                }

                                previousAlpha = alpha;
                            }
                            else if (alpha < previousAlpha)
                            {
                                if (counter > 0)
                                {
                                    counter--;
                                }
                                else
                                {
                                    counter = 2;
                                }
                                previousAlpha = alpha;
                            }
                        }
                    }
                    else
                    {
                        if (gender == "male")
                        {
                            if (GestureMsg == "right")
                            {

                                if (counter <= 2)
                                {
                                    counter++;
                                }
                                else
                                {
                                    counter = 0;
                                }

                                
                            }
                            else if (GestureMsg == "left")
                            {
                                if (counter > 0)
                                {
                                    counter--;
                                }
                                else
                                {
                                    counter = 2;
                                }
                                previousAlpha = alpha;
                            }
                        }
                        else
                        {
                            if (GestureMsg == "right")
                            {

                                if (counter <= 2)
                                {
                                    counter++;
                                }
                                else
                                {
                                    counter = 0;
                                }

                                
                            }
                            else if (GestureMsg == "left")
                            {
                                if (counter > 0)
                                {
                                    counter--;
                                }
                                else
                                {
                                    counter = 2;
                                }
                                
                            }
                        }
                    }

                    //Console.WriteLine($" Alpha = {alpha} || Previous = {previousAlpha} || diff = {alpha - previousAlpha} || counter = {counter}");
                    Pen pen = new Pen(Color.Yellow, 8);
                    if (gender == "male")
                    {
                        switch (counter)
                        {
                            case 0:
                                g.DrawRectangle(pen, rect1);
                                currentDisplayedSymbolID = 0;
                                break;
                            case 1:
                                g.DrawRectangle(pen, rect2);
                                currentDisplayedSymbolID = 13;
                                break;
                            case 2:
                                g.DrawRectangle(pen, rect3);
                                currentDisplayedSymbolID = 7;
                                break;
                        }
                    }
                    else
                    {
                        switch (counter)
                        {
                            case 0:
                                g.DrawRectangle(pen, rect1);
                                currentDisplayedSymbolID = 1;
                                break;
                            case 1:
                                g.DrawRectangle(pen, rect2);
                                currentDisplayedSymbolID = 16;
                                break;
                            case 2:
                                g.DrawRectangle(pen, rect3);
                                currentDisplayedSymbolID = 19;
                                break;
                        }
                    }
                }
            }
        }


        //g.DrawRectangle()
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
