import cv2
import numpy as np
import face_recognition
from tensorflow.keras.models import load_model
import socket

mySocket = socket.socket()
hostname="127.0.0.3"# 127.0.0.1 #0.0.0.0
port=5000
mySocket.bind((hostname,port))
mySocket.listen(1)
conn , addr = mySocket.accept()

# Load the pre-trained emotion detection model
emotion_model = load_model('model.h5')  # Replace with your emotion model file path
emotion_labels = ['Angry', 'Disgust', 'Fear', 'Happy', 'Sad', 'Surprise', 'Neutral']

# Load known face encodings and names
known_face_encodings = []
known_face_names = []

# Load face images and create encodings
known_images = {
    "Marena Anis": "marena.jpg",
    "Dr Ayman Ezzat": "Ayman.jpeg",
    "Dr Moamen Zaher": "Moamen.jpeg",
    "Dr Farah Darwish": "Farah.jpeg",
    "Shahd Tarek": "shahd.jpg",
    "Ahmed Wael": "ahmed.jpg",
    "Haidy Aboud": "haidy.jpg"
}

for name, image_path in known_images.items():
    image = face_recognition.load_image_file(image_path)
    encoding = face_recognition.face_encodings(image)[0]
    known_face_encodings.append(encoding)
    known_face_names.append(name)

# Initialize webcam
video_cap = cv2.VideoCapture(0)

while True:
    # Capture frame-by-frame
    ret, frame = video_cap.read()
    if not ret:
        break

    # Find all face locations and encodings in the current frame
    face_locations = face_recognition.face_locations(frame)
    face_encodings = face_recognition.face_encodings(frame, face_locations)

    for (top, right, bottom, left), face_encoding in zip(face_locations, face_encodings):
        # Identify the person
        matches = face_recognition.compare_faces(known_face_encodings, face_encoding)
        name = "Unknown"

        if True in matches:
            first_match_index = matches.index(True)
            name = known_face_names[first_match_index]
            msg2 =bytes(name, 'utf-8')
            conn.send(msg2)

        # Draw a rectangle around the face
        cv2.rectangle(frame, (left, top), (right, bottom), (0, 0, 255), 2)

        # Extract face ROI for emotion detection
        face_roi = frame[top:bottom, left:right]
        try:
            face_gray = cv2.cvtColor(face_roi, cv2.COLOR_BGR2GRAY)
            face_resized = cv2.resize(face_gray, (48, 48))  # Resize to model input size
            face_normalized = face_resized / 255.0
            face_input = np.expand_dims(face_normalized, axis=-1)
            face_input = np.expand_dims(face_input, axis=0)

            # Predict emotion
            emotion_prediction = emotion_model.predict(face_input, verbose=0)
            emotion_label = emotion_labels[np.argmax(emotion_prediction)]

            # Display name and emotion
            display_text = f"{name} - {emotion_label}"
            cv2.putText(frame, display_text, (left, top - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 255, 0), 2)
        except Exception as e:
            print(f"Error in emotion detection: {e}")

    # Display the video
    cv2.imshow("Face Recognition and Emotion Detection", frame)

    # Break loop on 'q' key press
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release resources
video_cap.release()
cv2.destroyAllWindows()
