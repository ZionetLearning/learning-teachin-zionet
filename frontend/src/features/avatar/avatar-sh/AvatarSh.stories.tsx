import { AvatarSh } from ".";
import type { Meta, StoryObj } from "@storybook/react-vite";

const meta: Meta<typeof AvatarSh> = {
  component: AvatarSh,
  title: "Features/Avatars/AvatarSh",
};

export default meta;

type Story = StoryObj<typeof AvatarSh>;

export const Default: Story = {
  render: () => (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
      }}
    >
      <AvatarSh />
    </div>
  ),
};
