import multiprocessing
import subprocess

def run_file(file_name):
    # Run the file using subprocess for better control
    subprocess.run(["python", file_name])

if __name__ == "__main__":
    # List of Python files to run
    files_to_run = [
        r"D:/Uni/semester 7/CS484/Project/Object_Detection/Detection.py",
        r"D:/Uni/semester 7/CS484/Project/Gestures_Part/templates/testwizcam.py"
    ]

    # Create and start processes
    processes = []
    for file in files_to_run:
        process = multiprocessing.Process(target=run_file, args=(file,))
        processes.append(process)
        process.start()

    print("All scripts finished execution!")
