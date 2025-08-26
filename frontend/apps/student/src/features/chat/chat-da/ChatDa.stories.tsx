import { Meta, StoryObj } from "@storybook/react-vite";

import { ChatDa } from ".";

const meta: Meta<typeof ChatDa> = {
  component: ChatDa,
  title: "Features/Chats/ChatDa",
  parameters: {
    layout: "fullscreen",
  },
};

export default meta;

type Story = StoryObj<typeof ChatDa>;

export const Default: Story = {
  render: () => (
    <div style={{ display: "flex", height: "100vh" }}>
      <div style={{ flex: 1, display: "flex", marginTop: "5%" }}>
        <ChatDa />
      </div>
    </div>
  ),
};
