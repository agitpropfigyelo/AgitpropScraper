# Use an official Python runtime as the base image
FROM python:3.12-slim

# Set the working directory in the container
WORKDIR /app

COPY . .
RUN pip install --upgrade pip
RUN pip install -r requirements.txt
RUN pip install https://huggingface.co/huspacy/hu_core_news_lg/resolve/main/hu_core_news_lg-3.8.1-py3-none-any.whl


# Expose the port the app runs on
EXPOSE 8000

# Set the entry point for the container
CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8000"]