from ultralytics import YOLO
import cv2
import socket

mySocket = socket.socket()
hostname="127.0.0.1"# 127.0.0.1 #0.0.0.0
port=4000
mySocket.bind((hostname,port))
mySocket.listen(1)
conn , addr = mySocket.accept() #creates a new socket (conn) dedicated to that specific client connection. 
#This new socket (conn) is now used exclusively for data exchange between the server and that client.
#This separation ensures that soc can continue listening for other clients while conn handles communication with the current client.
print("device connected")
# Initialize the webcam

cap = cv2.VideoCapture(0)  # Change `0` to another number if you have multiple webcams

# Check if the webcam is opened correctly
if not cap.isOpened():
    print("Error: Cannot access the webcam")
    exit()

# Load the trained YOLO model
model = YOLO("D:/Uni/semester 7/CS484/Project/Object_Detection/bestmodel.pt")  # Replace 'bestmodel.pt' with the path to your model file

print("Model loaded successfully!")

# Run the video capture loop
while True:
    
    # Read a frame from the webcam
    ret, frame = cap.read()

    # Check if the frame was captured correctly
    if not ret:
        print("Error: Failed to capture frame")
        break

    # Use the YOLO model to make predictions
    results = model(frame)

    # Annotate the frame with predictions
    annotated_frame = results[0].plot()  # Use `.plot()` to overlay bounding boxes and labels
    print("=========================")
    
    if len(results[0].boxes) > 0:  # Ensure at least one object is detected
        first_box = results[0].boxes[0]  # Get the first box
        class_id = int(first_box.cls)  # Class index
        confidence = first_box.conf  # Confidence score
        label = results[0].names[class_id]  # Class label
    #print(label)
    msg2 =bytes(label, 'utf-8')
    conn.send(msg2)

    # Display the frame with annotations
    cv2.imshow("YOLO Object Detection", annotated_frame)

    # Break the loop if the user presses 'q'
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release the webcam and close all OpenCV windows
cap.release()
cv2.destroyAllWindows()
    