import os
import cv2
import mediapipe as mp

# Initialize MediaPipe Pose estimator
mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose

pose = mp_pose.Pose(
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5)

def loop_files(directory, output_directory):
    # Create the output Python file to store the generated templates
    output_file = os.path.join(output_directory, "TestDollar.Py")
    f = open(output_file, "w")
    
    # Import necessary modules in the output file
    f.write("from dollarpy import Recognizer, Template, Point\n")
    
    # String to collect template names
    recstring = ""
    
    # Loop through each file in the given directory
    for file_name in os.listdir(directory):
        if os.path.isfile(os.path.join(directory, file_name)):
            if file_name.endswith(".mp4"):  # Only process .mp4 files
                print(f"Processing: {file_name}")
                
                # Create a name for the template based on the file name (without extension)
                template_name = file_name[:-4]
                recstring += template_name + ","
                
                # Write the template header to the file
                f.write(f"{template_name} = Template('{template_name}', [\n")
                
                # Process the video file to extract pose landmarks
                cap = cv2.VideoCapture(os.path.join(directory, file_name))
                framecnt = 0
                while cap.isOpened():
                    ret, frame = cap.read()
                    if not ret:
                        print("Can't receive frame (stream end?). Exiting ...")
                        break
                    
                    frame = cv2.resize(frame, (480, 320))  # Resize the frame
                    framecnt += 1
                    
                    try:
                        # Convert frame to RGB for MediaPipe processing
                        RGB = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                        results = pose.process(RGB)
                        
                        # Get the width and height of the image
                        image_height, image_width, _ = frame.shape
                        
                        # Extract wrist positions from the landmarks
                        right_wrist_x = int(results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].x * image_width)
                        right_wrist_y = int(results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].y * image_height)
                        left_wrist_x = int(results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].x * image_width)
                        left_wrist_y = int(results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].y * image_height)
                        
                        # Write the points to the template in the output file
                        f.write(f"Point({right_wrist_x},{right_wrist_y}, 1),\n")
                        f.write(f"Point({left_wrist_x},{left_wrist_y}, 1),\n")
                        
                        # Optionally, draw the landmarks on the frame for visualization
                        mp_drawing.draw_landmarks(frame, results.pose_landmarks, mp_pose.POSE_CONNECTIONS)
                        
                        # Display the frame (comment this out if you don't want to display during processing)
                        cv2.imshow('Output', frame)
                        
                    except Exception as e:
                        print(f"Error processing frame: {e}")
                        break
                    
                    # Break the loop if 'q' is pressed
                    if cv2.waitKey(1) == ord('q'):
                        break
                
                # Write the template footer to the file
                f.write("])\n")
                cap.release()
                cv2.destroyAllWindows()
    
    # Close the output file after writing all templates
    recstring = recstring.rstrip(",")  # Remove the trailing comma
    f.write(f"recognizer = Recognizer([{recstring}])\n")
    f.close()
    print(f"Template generation complete. Check the file {output_file}.")

# Example usage
directory_path = "videos/"  # Folder containing .mp4 video files
output_directory = "output/"  # Folder to save the generated Python file
loop_files(directory_path, output_directory)
