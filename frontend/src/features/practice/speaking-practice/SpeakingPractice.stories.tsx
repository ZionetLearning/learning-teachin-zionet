import { Meta, StoryObj } from "@storybook/react-vite";

import { SpeakingPractice } from ".";

const meta: Meta<typeof SpeakingPractice> = {
  component: SpeakingPractice,
  title: "Features/Practices/SpeakingPractice",
};

export default meta;

type Story = StoryObj<typeof SpeakingPractice>;

export const Default: Story = {
  render: () => <SpeakingPractice />,
};
