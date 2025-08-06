import { Meta, StoryObj } from "@storybook/react-vite";

import { AvatarDa } from ".";

const meta: Meta<typeof AvatarDa> = {
  component: AvatarDa,
  title: "Features/Avatars/AvatarDa",
  parameters: {
    layout: "fullscreen",
  },
};

export default meta;

type Story = StoryObj<typeof AvatarDa>;

export const Default: Story = {
  render: () => (
    <div
      style={{
        position: "absolute",
        width: "100%",
        height: "100%",
      }}
    >
      <AvatarDa />
    </div>
  ),
};
