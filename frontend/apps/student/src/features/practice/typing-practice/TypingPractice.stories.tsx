import { TypingPractice } from ".";
import type { Meta, StoryObj } from "@storybook/react";
const meta: Meta<typeof TypingPractice> = {
  component: TypingPractice,
  title: "Features/Practices/TypingPractice",
};

export default meta;

type Story = StoryObj<typeof TypingPractice>;

export const Default: Story = {
  render: () => (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        height: "100vh",
      }}
    >
      <TypingPractice />
    </div>
  ),
};
