import axios from "axios";

const API_KEY = import.meta.env.VITE_AZURE_OPENAI_KEY!;
const ENDPOINT = import.meta.env.VITE_AZURE_OPENAI_ENDPOINT!;
const DEPLOYMENT_NAME = import.meta.env.VITE_AZURE_OPENAI_DEPLOYMENT_NAME!;

const API_URL = `${ENDPOINT}/openai/deployments/${DEPLOYMENT_NAME}/chat/completions?api-version=2024-02-15-preview`;

export async function sendChatMessage(message: string): Promise<string> {
  const headers = {
    "Content-Type": "application/json",
    "api-key": API_KEY,
  };

  const data = {
    messages: [
      { role: "system", content: "You are a cool person" },
      { role: "user", content: message },
    ],
    max_tokens: 500,
  };

  const response = await axios.post(API_URL, data, { headers });
  return response.data.choices[0].message.content;
}
