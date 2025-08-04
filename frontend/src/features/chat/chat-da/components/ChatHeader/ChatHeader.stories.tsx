import { Meta, StoryObj } from "@storybook/react-vite";
import { ChatHeader } from ".";

const meta: Meta<typeof ChatHeader> = {
  title: "Features/Chats/ChatDa/Components/ChatHeader",
  component: ChatHeader,
  parameters: {
    layout: "fullscreen",
  },
};

export default meta;
type Story = StoryObj<typeof ChatHeader>;

export const Default: Story = {
  render: () => <ChatHeader />,
};
