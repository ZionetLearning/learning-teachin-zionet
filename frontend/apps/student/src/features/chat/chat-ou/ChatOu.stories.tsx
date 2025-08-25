import { ChatOu } from ".";
import type { Meta, StoryObj } from "@storybook/react-vite";

const meta: Meta<typeof ChatOu> = {
  component: ChatOu,
  title: "Features/Chats/ChatOu",
};

export default meta;

type Story = StoryObj<typeof ChatOu>;

export const Default: Story = {
  render: () => <ChatOu />,
};
