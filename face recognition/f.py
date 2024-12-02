import cv2
import face_recognition

#load known face encoding and names
known_face_encoding = []
known_face_name = []

#load known face and names 
known_marena_image = face_recognition.load_image_file("marena.jpg")
known_ayman_image = face_recognition.load_image_file("Ayman.jpeg")
known_moamen_image = face_recognition.load_image_file("Moamen.jpeg")
known_farah_image = face_recognition.load_image_file("Farah.jpeg")
known_shahd_image = face_recognition.load_image_file("shahd.jpg")
known_ahmed_image = face_recognition.load_image_file("ahmed.jpg")
known_haidy_image = face_recognition.load_image_file("haidy.jpg")
#add more varibales according to how man person you want to recognize.

known_marena_encoding = face_recognition.face_encodings(known_marena_image)[0]
known_ayman_encoding = face_recognition.face_encodings(known_ayman_image)[0]
known_moamen_encoding = face_recognition.face_encodings(known_moamen_image)[0]
known_farah_encoding = face_recognition.face_encodings(known_farah_image)[0]
known_shahd_encoding = face_recognition.face_encodings(known_shahd_image)[0]
known_ahmed_encoding = face_recognition.face_encodings(known_ahmed_image)[0]
known_haidy_encoding = face_recognition.face_encodings(known_haidy_image)[0]
#and so on with other varibales of images.

known_face_encoding.append(known_marena_encoding)
known_face_encoding.append(known_ayman_encoding)
known_face_encoding.append(known_moamen_encoding)
known_face_encoding.append(known_farah_encoding)
known_face_encoding.append(known_shahd_encoding)
known_face_encoding.append(known_ahmed_encoding)
known_face_encoding.append(known_haidy_encoding)
#and so on with other varibales of encoding.

known_face_name.append("Marena Anis")
known_face_name.append("Dr Ayman Ezzat")
known_face_name.append("Dr Moamen Zaher")
known_face_name.append("Dr Farah Darwish")
known_face_name.append("Shahd Tarek")
known_face_name.append("Ahmed Wael")
known_face_name.append("Haidy Aboud")
#and so on with other varibales of names.

video_cap = cv2.VideoCapture(0)

while True:
    #capture frame by frame
    ret, frame = video_cap.read()
    
    #find all faces location in the current frame
    face_locations = face_recognition.face_locations(frame)
    face_encodings = face_recognition.face_encodings(frame, face_locations)

    #loop through each face found in the frame
    for(top, right, bottom, left), face_encoding in zip(face_locations, face_encodings):
        #check if the face matches known faces
        matches = face_recognition.compare_faces(known_face_encoding, face_encoding)
        name= "unknown"

        if True in matches:
            first_match_index = matches.index(True)
            name = known_face_name[first_match_index]

        cv2.rectangle(frame, (left,top), (right,bottom), (0, 0 ,255), 2)
        cv2.putText(frame, name, (left, top - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 0, 255), 2)

    cv2.imshow("Video", frame)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

video_cap.release()
cv2.destroyAllWindows()





