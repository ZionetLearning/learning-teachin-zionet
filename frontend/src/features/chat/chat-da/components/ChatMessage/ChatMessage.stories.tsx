import { Meta, StoryObj } from "@storybook/react-vite";

import { ChatMessage } from ".";
import { Message } from "@/types";

const meta: Meta<typeof ChatMessage> = {
  component: ChatMessage,
  title: "Features/Chats/ChatDa/Components/ChatMessage",
  parameters: {
    layout: "fullscreen",
  },
  decorators: [
    (Story) => (
      <div
        style={{
          display: "flex",
          width: "100vw",
          height: "100vh",
          alignItems: "center",
          justifyContent: "center",
          flexDirection: "column",
          maxWidth: "600px",
          margin: "0 auto",
          backgroundColor: "#f5f5f5",
          paddingLeft: "20px",
          paddingRight: "20px",
        }}
      >
        <Story />
      </div>
    ),
  ],
};

export default meta;

type Story = StoryObj<typeof ChatMessage>;

export const Bot: Story = {
  args: {
    message: {
      id: "1",
      text: "This is the bot message",
      sender: "bot",
    } as Message,
  },
};

export const User: Story = {
  args: {
    message: {
      id: "2",
      text: "This is the user message",
      sender: "user",
    } as Message,
  },
};
