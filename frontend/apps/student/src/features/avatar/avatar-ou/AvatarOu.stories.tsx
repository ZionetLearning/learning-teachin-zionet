import { AvatarOu } from ".";
import type { Meta, StoryObj } from "@storybook/react";
const meta: Meta<typeof AvatarOu> = {
  component: AvatarOu,
  title: "Features/Avatars/AvatarOu",
};

export default meta;

type Story = StoryObj<typeof AvatarOu>;

export const Default: Story = {
  render: () => (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
      }}
    >
      <AvatarOu />
    </div>
  ),
};
