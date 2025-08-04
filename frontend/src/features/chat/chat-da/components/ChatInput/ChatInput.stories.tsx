import { Meta, StoryObj } from "@storybook/react-vite";
import { useArgs } from "storybook/internal/preview-api";

import { ChatInput } from ".";

const meta: Meta<typeof ChatInput> = {
  component: ChatInput,
  title: "Features/Chats/ChatDa/Components/ChatInput",
  parameters: {
    layout: "centered",
  },
  argTypes: {
    input: {
      control: "text",
    },
    setInput: {
      action: "setInput",
    },
  },
};

export default meta;
type Story = StoryObj<typeof ChatInput>;

const Template: Story = {
  render: (args) => {
    const [{ input }, updateArgs] = useArgs();
    return (
      <ChatInput
        {...args}
        input={input}
        setInput={(value) => updateArgs({ input: value })}
        sendMessage={() => updateArgs({ input: "" })}
      />
    );
  },
};

export const Default: Story = {
  ...Template,
  args: {
    disabled: false,
  },
};

export const Disabled: Story = {
  ...Template,
  args: {
    disabled: true,
  },
};
