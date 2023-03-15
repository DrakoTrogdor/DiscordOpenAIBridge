# Discord OpenAI Bridge

Discord OpenAI Bridge is a Discord bot that leverages the OpenAI GPT-3.5 API to translate user input into a pirate lingo or Zapp Brannigan style, using specific directives provided in a JSON configuration file. This makes your chat experience more fun and engaging.

## Features

- Translate user messages into pirate language using the `/tlap` command
- Works with Discord.NET library and C#

## Getting Started

### Prerequisites

- [NET 7.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com/) (optional, for development purposes)

### Installation

1. Clone the repository:

   ```shell
   git clone https://github.com/DrakoTrogdor/DiscordOpenAIBridge.git
   ```

2. Navigate to the project directory:

   ```shell
   cd Discord-Pirate-Translator
   ```

3. Install the required NuGet packages:

   ```shell
   dotnet restore
   ```

4. Add your ChatGPT API key and Discord bot token to the `config.json` file:

   ```json
   {
     "ChatGptApiKey": "your-chatgpt-api-key",
     "DiscordToken": "your-discord-bot-token"
   }
   ```

5. Build and run the project:

   ```shell
   dotnet build
   dotnet run
   ```

### Installing C# packages

When working on a C# project, you might find that your project already has package references specified in the `.csproj` file. These references inform the build system about the required dependencies for your project. However, just having the references in the `.csproj` file does not mean the packages are installed on your local development environment.

To ensure that the referenced packages are available for your project to use, you need to install them. Installing packages fetches the required files and makes them available for your project to build and run successfully. This is particularly important when collaborating with others, as they might have added new package references that your development environment doesn't have yet.

Follow the instructions below to install the necessary packages:

1. Open your terminal or command prompt.
2. Run the following commands:

   ```shell
   dotnet add package Discord.Net --version 3.9.0
   dotnet add package Microsoft.Extensions.Configuration --version 7.0.0
   dotnet add package Microsoft.Extensions.Configuration.Json --version 7.0.0
   dotnet add package Microsoft.Extensions.DependencyInjection --version 7.0.0
   dotnet add package Microsoft.Extensions.Logging --version 7.0.0
   dotnet add package Microsoft.Extensions.Logging.Console --version 7.0.0
   dotnet add package Microsoft.Extensions.Logging.Debug --version 7.0.0
   dotnet add package Serilog.AspNetCore
   dotnet add package Serilog.Sinks.File
   dotnet add package Serilog.Sinks.Console
   ```

These commands will install the specified packages and make them available for your project.

## Usage

Invite the bot to your Discord server, and use the `/tlap` command followed by your message to translate it into pirate language.

Example:

   ```plaintext
   /tlap Hello there!
   ```

The bot will respond with the translated message in the chat channel.

## How to Obtain a Discord Bot Token

Follow these simple steps to obtain a Discord bot token:

### Step 1: Create a Discord Application

1. Go to the [Discord Developer Portal](https://discord.com/developers/applications).
2. Log in with your Discord account if you haven't already.
3. Click on the `New Application` button in the top right corner.
4. Enter a name for your application and click `Create`.

### Step 2: Add a Bot to Your Application

1. Navigate to the `Bot` tab on the left side of the application page.
2. Click the `Add Bot` button.
3. Confirm by clicking `Yes, do it!`.

### Step 3: Get the Bot Token

1. On the `Bot` tab, locate the `TOKEN` section.
2. Click on the `Copy` button to copy the token to your clipboard.

**Important:** Keep your bot token secret! Do not share it with anyone or post it publicly.

### Step 4: Invite the Bot to Your Server

1. Go to the `OAuth2` tab on the left side of the application page.
2. Scroll down to the `Scopes` section and check the `bot` checkbox.
3. Scroll down to the `Bot Permissions` section and select the desired permissions for your bot.
4. Copy the generated URL from the `Scopes` section.
5. Open the URL in a new browser tab and choose the server you want to add the bot to.
6. Click `Authorize` and complete the captcha if prompted.

## How to Obtain an OpenAI ChatGPT API Key

Follow these simple steps to obtain an OpenAI ChatGPT API key:

### Step 1: Create an OpenAI Account

1. Go to the [OpenAI Platform](https://platform.openai.com/signup).
2. Sign up for a new account or log in if you already have one.

### Step 2: Access the API Key

1. Once logged in, navigate to the [API Keys](https://platform.openai.com/api-keys) section from the dashboard.
2. If you don't have an API key yet, click on the `+ Create API Key` button.
3. Enter a name for your API key (e.g., "My ChatGPT Key") and click `Create`.

### Step 3: Get the API Key

1. Locate your new API key in the list and click the `Show` button to reveal the key.
2. Click on the `Copy` button to copy the API key to your clipboard.

**Important:** Keep your API key secret! Do not share it with anyone or post it publicly.

## Handling Sensitive Information

To prevent accidentally uploading sensitive information like the ChatGPT API key and Discord bot token, follow these steps to set up smudge and clean filters:

1. Create a `.gitattributes` file in your project's root directory with the following content:

   ```plaintext
   config.json filter=secrets
   ```

   This tells Git to apply the `secrets` filter to the `config.json` file.

2. Configure the smudge and clean filters by running the following commands:

   ```shell
   git config filter.secrets.clean "sed 's/\"ChatGptApiKey\": \".*\"/\"ChatGptApiKey\": \"your-chatgpt-api-key\"/g; s/\"DiscordToken\": \".*\"/\"DiscordToken\":   \"your-discord-bot-token\"/g'"
   git config filter.secrets.smudge "sed 's/\"ChatGptApiKey\": \"your-chatgpt-api-key\"/\"ChatGptApiKey\": \"$(git config chatgpt.apikey)\"/g; s/\"DiscordToken\":   \"your-discord-bot-token\"/\"DiscordToken\": \"$(git config discord.token)\"/g'"
   ```

   These commands configure the `secrets` filter to replace the actual API key and bot token with placeholder values when committing the code (clean filter) and replace the placeholder values with the real values when checking out the code (smudge filter).

3. Store your actual ChatGPT API key and Discord bot token in your Git configuration:

   ```shell
   git config chatgpt.apikey "your_actual_chatgpt_api_key"
   git config discord.token "your_actual_discord_bot_token"
   ```

   Replace `your_actual_chatgpt_api_key` and `your_actual_discord_bot_token` with your real API key and bot token.

Now, whenever you commit your code, Git will replace the actual API key and bot token with the placeholder values in the `config.json` file. When you check out the code, Git will replace the placeholder values with the real values stored in your Git configuration.

Please note that this method is not foolproof and can be circumvented if someone knows the real values. It is always recommended to use proper secret management solutions, such as environment variables or secret management services, to handle sensitive information in your projects.

## Contributing

If you'd like to contribute to the project, please follow the standard GitHub workflow:

1. Fork the repository
2. Create a new branch for your feature or bugfix
3. Commit your changes
4. Push your changes to your fork
5. Open a pull request to the original repository

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
