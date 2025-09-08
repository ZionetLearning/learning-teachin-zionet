import React, { useState } from "react";
import type { Meta, StoryObj } from "@storybook/react";
import { Dropdown } from "./index";

const meta: Meta<typeof Dropdown> = {
  title: "Libs/UI/Components/Dropdown",
  component: Dropdown,
  args: {
    name: "role",
    label: "Role",
    value: "",
    options: [
      { value: "student", label: "Student" },
      { value: "teacher", label: "Teacher" },
      { value: "admin", label: "Admin" },
    ],
    disabled: false,
    error: false,
    helperText: "",
  },
  argTypes: {
    onChange: { action: "changed" },
    name: { control: "text" },
    label: { control: "text" },
    value: { control: "text" },
    disabled: { control: "boolean" },
    error: { control: "boolean" },
    helperText: { control: "text" },
    "data-testid": { control: "text" },
  },
  decorators: [
    (Story) => (
      <div style={{ maxWidth: 360, padding: 16, background: "#fff" }}>
        <Story />
      </div>
    ),
  ],
  parameters: {
    controls: { expanded: true },
  },
};

export default meta;
type Story = StoryObj<typeof Dropdown>;

const Controlled = (args: React.ComponentProps<typeof Dropdown>) => {
  const [val, setVal] = useState(args.value ?? "");
  return (
    <Dropdown
      {...args}
      value={val}
      onChange={(v) => {
        setVal(v);
        args.onChange?.(v);
      }}
    />
  );
};

export const Default: Story = {
  render: (args) => <Controlled {...args} />,
};

export const InitialValueAndLabel: Story = {
  args: { value: "student" },
  render: (args) => <Controlled {...args} />,
};

export const ErrorState: Story = {
  args: {
    error: true,
    helperText: "This field is required",
  },
  render: (args) => <Controlled {...args} />,
};

export const Disabled: Story = {
  args: { disabled: true },
  render: (args) => <Controlled {...args} />,
};

export const RTL_Hebrew: Story = {
  args: {
    label: "תפקיד",
    options: [
      { value: "student", label: "תלמיד" },
      { value: "teacher", label: "מורה" },
      { value: "admin", label: "מנהל" },
    ],
  },
  decorators: [
    (Story) => (
      <div dir="rtl" style={{ maxWidth: 360, padding: 16, background: "#fff" }}>
        <Story />
      </div>
    ),
  ],
  render: (args) => <Controlled {...args} />,
};
