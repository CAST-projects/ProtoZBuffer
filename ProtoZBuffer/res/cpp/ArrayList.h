#pragma once
#include <vector>

%NAMESPACE_BEGIN%

template <class T> class ArrayList
{
private:

    std::vector<T*> m_map;
    typedef typename std::vector<T*>::iterator iterator;

public:

    T* get(std::size_t index)
    {
        return index < size() ? m_map.at(index) : nullptr;
    }

    void set(std::size_t index, std::unique_ptr<T>& value)
    {
        // pad with nullptrs until m_map.size() >= index + 1 
        if (index >= m_map.size())
            m_map.insert(m_map.end(), index + 1 - m_map.size(), nullptr);
        m_map[index] = value.release(); // takes ownership
    }

    std::size_t size() const
    {
        return m_map.size();
    }

    void clear()
    {
        for (auto item : m_map)
        {
            delete item;
        }

        m_map.clear();
    }

    ~ArrayList() { clear(); }

};

%NAMESPACE_END%
