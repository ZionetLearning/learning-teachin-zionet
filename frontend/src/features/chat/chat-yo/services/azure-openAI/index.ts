import axios from "axios";

export const askAzureOpenAI = async (
  userMessage: string,
  systemInstructions?: string,
): Promise<string> => {
  const API_KEY = import.meta.env.VITE_AZURE_OPENAI_KEY!;
  const ENDPOINT = import.meta.env.VITE_AZURE_OPENAI_ENDPOINT!;
  const DEPLOYMENT_NAME = import.meta.env.VITE_AZURE_OPENAI_DEPLOYMENT_NAME!;

  const API_URL = `${ENDPOINT}/openai/deployments/${DEPLOYMENT_NAME}/chat/completions?api-version=2024-02-15-preview`;
  try {
    const response = await axios.post(
      API_URL,
      {
        messages: [
          {
            role: "system",
            content: systemInstructions ?? "You are a helpful assistant.",
          },
          { role: "user", content: userMessage },
        ],
        max_tokens: 200,
      },
      {
        headers: {
          "Content-Type": "application/json",
          "api-key": API_KEY,
        },
      },
    );

    return response.data.choices[0]?.message?.content ?? "";
  } catch (error) {
    console.error("Azure OpenAI API Error:", error);
    return "Something went wrong.";
  }
};
