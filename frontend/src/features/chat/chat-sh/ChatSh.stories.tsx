import { ChatSh } from ".";
import type { Meta, StoryObj } from "@storybook/react-vite";

const meta: Meta<typeof ChatSh> = {
  component: ChatSh,
  title: "Features/Chats/ChatSh",
};

export default meta;

type Story = StoryObj<typeof ChatSh>;

export const Default: Story = {
  render: () => <ChatSh />,
};
