# AgitpropScraper

AgitpropScraper is a comprehensive scraping and processing framework designed to collect, parse, and analyze data from various sources. The project is modular, with distinct components for scraping, data processing, and natural language processing (NLP).

## Project Structure

The repository is organized into the following main components:

- **Agitprop.AppHost**: Hosts the main application logic and configuration files.
- **Agitprop.ConsoleToolKit**: Provides command-line tools for managing scraping jobs and retrying failed tasks.
- **Agitprop.Consumer**: Handles data consumption and processing pipelines.
- **Agitprop.Core**: Contains core abstractions, models, and utilities used across the project.
- **Agitprop.Infrastructure**: Implements infrastructure-level services like page loading and proxy management.
- **Agitprop.Infrastructure.Puppeteer**: Integrates Puppeteer for advanced web scraping capabilities.
- **Agitprop.NLPService**: A Python-based service for natural language processing, including named entity recognition.
- **Agitprop.RssFeedReader**: Reads and processes RSS feeds.
- **Agitprop.Scraper.Sinks.Newsfeed**: Processes and stores scraped newsfeed data.
- **Agitprop.ServiceDefaults**: Provides default configurations and shared services.

## Key Features

- **Web Scraping**: Supports scraping with Puppeteer and custom proxy management.
- **NLP Integration**: Includes a Python-based NLP service for advanced text analysis.
- **Command-Line Tools**: Offers tools for managing scraping jobs and retries.
- **Modular Design**: Each component is self-contained and reusable.

## Getting Started

### Prerequisites

- .NET SDK
- Python 3.x
- Docker (for containerized services)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/AgitpropScraper.git
   ```
2. Navigate to the project directory:
   ```bash
   cd AgitpropScraper
   ```
3. Install dependencies for the NLP service:
   ```bash
   cd Agitprop.NLPService
   pip install -r requirements.txt
   ```

### Running the Project

1. Build the solution:
   ```bash
   dotnet build Agitprop.Scraper.sln
   ```
2. Run the main application:
   ```bash
   dotnet run --project Agitprop.AppHost
   ```
3. Start the NLP service:
   ```bash
   cd Agitprop.NLPService
   python app.py
   ```

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

## Contact

For questions or support, please contact [your-email@example.com].