import { ErrorBoundary } from "react-error-boundary";

interface PanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

const Panel = (props: PanelProps) => {
  const { children, value, index } = props;

  return (
    <ErrorBoundary fallback={<div>Something went wrong</div>}>
      <div
        role="panel"
        hidden={value !== index}
        id={`panel-${index}`}
        aria-labelledby={`tab-${index}`}
      >
        {value === index && <div>{children}</div>}
      </div>
    </ErrorBoundary>
  );
};

export default Panel;
