# Use an official Python runtime as the base image
FROM python:3.9-slim

# Set the working directory in the container
WORKDIR /app

COPY . .
RUN pip install --no-cache-dir -r requirements.txt

# Set the entry point for the container
CMD ["python", "app.py"]
