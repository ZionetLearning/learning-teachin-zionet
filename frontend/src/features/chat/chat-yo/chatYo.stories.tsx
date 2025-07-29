import { ChatYo } from "./index";
import type { Meta, StoryObj } from "@storybook/react";

const meta: Meta<typeof ChatYo> = {
    component: ChatYo,
    title: "Features/ChatYo",
};

export default meta;

type Story = StoryObj<typeof ChatYo>;

export const Default: Story = {
    render: () => <ChatYo />,
};
