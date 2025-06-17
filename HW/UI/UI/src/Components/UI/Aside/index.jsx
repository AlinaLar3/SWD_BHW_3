import { AsideHeader } from '@gravity-ui/navigation';
import { LayoutList, Persons } from '@gravity-ui/icons';

const Aside = ({ children }) => {
  return (
    <AsideHeader
      headerDecoration={true}
      compact={true}
      hideCollapseButton={true}
      menuItems={[
        {
          id: 'transactions',
          icon: LayoutList,
          title: 'Список транзакций',
          link: '/transactions',
        },
        {
          id: 'divider',
          type: 'divider',
          title: '',
        },
        {
          id: 'accounts',
          icon: Persons,
          title: 'Список счетов',
          link: '/accounts',
        },
      ]}
      renderContent={() => children}
    />
  );
};

export default Aside;
