import type { Meta, StoryObj } from "@storybook/react";
import { Button } from "./index";
import { ThemeProvider, CssBaseline, createTheme } from "@mui/material";

const meta: Meta<typeof Button> = {
  title: "Libs/UI/Components/Button",
  component: Button,
  args: {
    children: "SEND",
    disabled: false,
  },
  argTypes: {
    onClick: { action: "clicked" },
    disabled: { control: "boolean" },
    children: { control: "text" },
  },
  decorators: [
    (Story) => (
      <ThemeProvider theme={createTheme()}>
        <CssBaseline />
        <Story />
      </ThemeProvider>
    ),
  ],
  parameters: {
    controls: { expanded: true },
  },
};

export default meta;
type Story = StoryObj<typeof Button>;

export const Default: Story = {};

export const Disabled: Story = {
  args: { disabled: true },
};

export const CustomLabel: Story = {
  args: { children: "Save changes" },
};
