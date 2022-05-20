/*

Copyright 2010-2016 Thomas Rizos. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY COPYRIGHT HOLDER ``AS IS'' AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
EVENT SHALL COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

#ifndef __CIRCULARBUFFER_H__
#define __CIRCULARBUFFER_H__


#include <vector>
#include <iterator>
#include <assert.h>


namespace std {

	template<typename T, typename A> class circular_buffer_iterator;
	template<typename T, typename A> class circular_buffer_const_iterator;

	template<typename T, typename A = std::allocator<T> >
	class circular_buffer
	{
	public:
		typedef circular_buffer<T, A> self_type;
		typedef A allocator_type;
		typedef typename allocator_type::size_type size_type;
		typedef typename allocator_type::difference_type difference_type;
		typedef typename allocator_type::pointer pointer;
		typedef typename allocator_type::const_pointer const_pointer;
		typedef typename allocator_type::reference reference;
		typedef typename allocator_type::const_reference const_reference;
		typedef typename allocator_type::value_type value_type;

		circular_buffer() : m_next(0), m_bufferSize(0) {}

		explicit circular_buffer(size_type bufferSize) : m_next(0),m_bufferSize(bufferSize)
		{
			m_values.reserve(m_bufferSize);
		}

		explicit circular_buffer(const allocator_type& a) :  m_values(a), m_next(0), m_bufferSize(0) {}

		explicit circular_buffer(size_type bufferSize, size_type count) :  m_next(0), m_bufferSize(bufferSize)
		{
			m_values.reserve(m_bufferSize);
			if (m_bufferSize < count)
				count = m_bufferSize;
			m_values.resize(count);
		}

		circular_buffer(size_type bufferSize, size_type count, const_reference value) :  m_next(0), m_bufferSize(0)
		{
			m_values.reserve(m_bufferSize);
			if (m_bufferSize < count)
				count = m_bufferSize;
			m_values.resize(count, value);
		}

		circular_buffer(size_type bufferSize, size_type count, const_reference value, const allocator_type& a) :  m_values(a), m_next(0), m_bufferSize(0)
		{
			m_values.reserve(m_bufferSize);
			if (m_bufferSize < count)
				count = m_bufferSize;
			m_values.resize(count, value);
		}

		circular_buffer(const circular_buffer& c) : m_values(c.m_values), m_next(c.m_next), m_bufferSize(c.m_bufferSize) {}

		size_type max_size() const
		{
			return m_bufferSize;
		}

		size_type capacity() const
		{
			return m_bufferSize;
		}

		void reserve(size_type bufferSize)
		{
			if ((bufferSize >= m_bufferSize && m_bufferSize != 0) || bufferSize >= m_values.size())
			{
				m_bufferSize = bufferSize;
				m_values.reserve(bufferSize);
			}
			else
			{
				// Need to use resize to shrink already allocated buffers
				assert(false);
			}
		}

		void resize(size_type bufferSize);

		void resize(size_type bufferSize, size_type count);

		void resize(size_type bufferSize, size_type count, const_reference value);

		size_type size() const
		{
			return m_values.size();
		}

		bool empty() const { return m_values.empty(); }

		void clear() 
		{
			m_next = 0;
			m_values.clear();
			m_values.reserve(m_bufferSize);
		}

		const_reference at(size_type pos) const
		{
			return m_values[(m_next + pos) % (m_values.size())];
		}

		reference at(size_type pos)
		{
			return m_values[(m_next + pos) % (m_values.size())];
		}


		const_reference operator[](size_type pos) const
		{
			return m_values[(m_next + pos) % (m_values.size())];
		}

		reference operator[](size_type pos)
		{
			return m_values[(m_next + pos) % (m_values.size())];
		}

		void push_back(const_reference value)
		{
			// With bufferSize set to zero this works like a normal vector (no wrapping at all)
			if (m_values.size() < m_bufferSize || m_bufferSize == 0)
			{
				m_values.push_back(value);
				m_next = 0;
			}
			else
			{
				m_values[m_next++ % (m_values.size())] = value;
				m_next %= (m_values.size());
			}
		}
		
		bool is_circular()
		{
			if(m_values.size() == m_bufferSize && m_next == 0)
			{
				return true;
			}
			return false;
		}

		void swap(circular_buffer& c)
		{
			size_type tempNext = m_next;
			size_type tempBufferSize = m_bufferSize;
			m_next = c.m_next;
			m_bufferSize = c.m_bufferSize;
			m_values.swap(c.m_values);
			c.m_next = tempNext;
			c.m_bufferSize = tempBufferSize;
		}

		reference front() { return this->operator[](0); }
		const_reference front() const { return this->operator[](0); }
		reference back() { return this->operator[](m_values.size()-1); }
		const_reference back() const { return this->operator[](m_values.size()-1); }


		typedef circular_buffer_iterator<T, A> iterator;
		typedef circular_buffer_const_iterator<T, A> const_iterator;

		iterator begin();
		const_iterator begin() const;
		iterator end();
		const_iterator end() const;

		typedef std::reverse_iterator<iterator> reverse_iterator;
		typedef std::reverse_iterator<const_iterator> const_reverse_iterator;

		reverse_iterator rbegin();
		const_reverse_iterator rbegin() const;
		reverse_iterator rend();
		const_reverse_iterator rend() const;

        inline T* buffer() { return &m_values[0];}
	protected:
		typedef std::vector<T, A> vector_type;

		vector_type m_values;
		size_type m_next;
		size_type m_bufferSize;
	};


	template<typename T, typename A> 
	class circular_buffer_iterator : public std::iterator<std::random_access_iterator_tag,
														  typename circular_buffer<T, A>::value_type,
														  typename circular_buffer<T, A>::difference_type,
														  typename circular_buffer<T, A>::pointer,
														  typename circular_buffer<T, A>::reference>
	{
	public:
		typedef typename circular_buffer<T, A>::size_type size_type;
        typedef typename circular_buffer<T, A>::reference reference;
        typedef typename circular_buffer<T, A>::difference_type difference_type;
		circular_buffer_iterator() : m_p(NULL), m_i(0) {}
		circular_buffer_iterator(const circular_buffer_iterator& c) : m_p(c.m_p), m_i(c.m_i) {}
		~circular_buffer_iterator() { m_p = NULL; }

		circular_buffer_iterator& operator=(const circular_buffer_iterator& c)
		{
			m_p = c.m_p;
			m_i = c.m_i;
			return *this;
		}

		circular_buffer_iterator& operator++() { ++m_i; return *this; }
		circular_buffer_iterator operator++(int) { circular_buffer_iterator i(*this); ++m_i; return i; }
		circular_buffer_iterator& operator--() { --m_i; return *this; }
		circular_buffer_iterator operator--(int) { circular_buffer_iterator i(*this); --m_i; return i; }

		reference operator*() { assert(m_p); return (*m_p)[m_i]; }
		reference operator->() { assert(m_p); return (*m_p)[m_i]; }

		circular_buffer_iterator& operator += (difference_type offset) { m_i += offset; return *this; }
		circular_buffer_iterator& operator -= (difference_type offset)  { m_i -= offset; return *this; }
		circular_buffer_iterator operator + (difference_type offset) const { circular_buffer_iterator i(*this); i += offset; return i; }
		circular_buffer_iterator operator - (difference_type offset) const { circular_buffer_iterator i(*this); i -= offset; return i; }

		difference_type operator - (const circular_buffer_iterator &rhs) const
		{
			assert(m_p);
			assert(rhs.m_p);
			assert(m_p == rhs.m_p);
			return m_i - rhs.m_i;
		}
		
		bool operator < (const circular_buffer_iterator &rhs) const
		{
			assert(m_p);
			assert(rhs.m_p);
			assert(m_p == rhs.m_p);
			
			return m_i < rhs.m_i;
		}
	
		bool operator <= (const circular_buffer_iterator &rhs) const
		{
			assert(m_p);
			assert(rhs.m_p);
			assert(m_p == rhs.m_p);
			
			return m_i <= rhs.m_i;
		}
		
		bool operator == (const circular_buffer_iterator &rhs) const
		{
			assert(m_p);
			assert(rhs.m_p);
			assert(m_p == rhs.m_p);
			
			return m_i == rhs.m_i;
		}
	
		bool operator != (const circular_buffer_iterator &rhs) const { return !(*this == rhs); }
		bool operator >  (const circular_buffer_iterator &rhs) const { return !(*this <= rhs); }
		bool operator >= (const circular_buffer_iterator &rhs) const { return !(*this <  rhs); }

		friend circular_buffer<T, A>;
		friend circular_buffer_const_iterator<T, A>;
	protected:
		circular_buffer_iterator(circular_buffer<T,A>* p, size_type i) : m_p(p), m_i(i) {}

	private:
		circular_buffer<T, A>* m_p;
		size_type m_i;
	};


	template<typename T, typename A> 
	class circular_buffer_const_iterator : public std::iterator<std::random_access_iterator_tag,
														  typename circular_buffer<T, A>::value_type,
														  typename circular_buffer<T, A>::difference_type,
														  typename circular_buffer<T, A>::const_pointer,
														  typename circular_buffer<T, A>::const_reference>
	{
	public:
		typedef typename circular_buffer<T, A>::size_type size_type;
        
        typedef typename circular_buffer<T, A>::reference reference;
        typedef typename circular_buffer<T, A>::difference_type difference_type;
		circular_buffer_const_iterator() : m_p(NULL), m_i(0) {}
		circular_buffer_const_iterator(const circular_buffer_iterator<T,A>& c) : m_p(c.m_p), m_i(c.m_i) {}
		circular_buffer_const_iterator(const circular_buffer_const_iterator& c) : m_p(c.m_p), m_i(c.m_i) {}
		~circular_buffer_const_iterator() { m_p = NULL; }

		circular_buffer_const_iterator& operator=(const circular_buffer_const_iterator& c)
		{
			m_p = c.m_p;
			m_i = c.m_i;
			return *this;
		}

		circular_buffer_const_iterator& operator=(const circular_buffer_iterator<T,A>& c)
		{
			m_p = c.m_p;
			m_i = c.m_i;
			return *this;
		}

		circular_buffer_const_iterator& operator++() { ++m_i; return *this; }
		circular_buffer_const_iterator operator++(int) { circular_buffer_const_iterator i(*this); ++m_i; return i; }
		circular_buffer_const_iterator& operator--() { --m_i; return *this; }
		circular_buffer_const_iterator operator--(int) { circular_buffer_const_iterator i(*this); --m_i; return i; }

		reference operator*() const { assert(m_p); return (*m_p)[m_i]; }
		reference operator->() const { assert(m_p); return (*m_p)[m_i]; }

		circular_buffer_const_iterator& operator += (difference_type offset) { m_i += offset; return *this; }
		circular_buffer_const_iterator& operator -= (difference_type offset)  { m_i -= offset; return *this; }
		circular_buffer_const_iterator operator + (difference_type offset) const { circular_buffer_const_iterator i(*this); i += offset; return i; }
		circular_buffer_const_iterator operator - (difference_type offset) const { circular_buffer_const_iterator i(*this); i -= offset; return i; }

		difference_type operator - (const circular_buffer_const_iterator &rhs) const
		{
			assert(m_p);
			assert(rhs.m_p);
			assert(m_p == rhs.m_p);
			return m_i - rhs.m_i;
		}
		
		bool operator < (const circular_buffer_const_iterator &rhs) const
		{
			assert(m_p);
			assert(rhs.m_p);
			assert(m_p == rhs.m_p);
			
			return m_i < rhs.m_i;
		}
	
		bool operator <= (const circular_buffer_const_iterator &rhs) const
		{
			assert(m_p);
			assert(rhs.m_p);
			assert(m_p == rhs.m_p);
			
			return m_i <= rhs.m_i;
		}
		
		bool operator == (const circular_buffer_const_iterator &rhs) const
		{
			assert(m_p);
			assert(rhs.m_p);
			assert(m_p == rhs.m_p);
			
			return m_i == rhs.m_i;
		}
	
		bool operator != (const circular_buffer_const_iterator &rhs) const { return !(*this == rhs); }
		bool operator >  (const circular_buffer_const_iterator &rhs) const { return !(*this <= rhs); }
		bool operator >= (const circular_buffer_const_iterator &rhs) const { return !(*this <  rhs); }

		friend circular_buffer<T, A>;
	protected:
		circular_buffer_const_iterator(const circular_buffer<T,A>* p, size_type i) : m_p(p), m_i(i) {}

	private:
		const circular_buffer<T, A>* m_p;
		size_type m_i;
	};

	template<typename T, typename A> inline
	void circular_buffer<T, A>::resize(size_type bufferSize)
	{
		resize(bufferSize, m_values.size() < bufferSize ? m_values.size() : bufferSize, value_type());
	}

	template<typename T, typename A> inline
	void circular_buffer<T, A>::resize(size_type bufferSize, size_type count)
	{
		resize(bufferSize, count, value_type());
	}

	template<typename T, typename A> inline
	void circular_buffer<T, A>::resize(size_type bufferSize, size_type count, const_reference value)
	{
		assert(count <= bufferSize);
		if (bufferSize > m_bufferSize)
		{
			// increase buffer size
			if (m_values.size() < m_bufferSize && m_bufferSize != 0)
			{
				// buffer not filled yet, so just expand it
				m_bufferSize = bufferSize;
				m_values.reserve(bufferSize);
				if (m_values.size() < count)
					m_values.resize(count, value);
			}
			else
			{
				// need to move the elements to keep age order
				vector_type v;
				v.reserve(bufferSize);
				size_type n = m_values.size() < count ? m_values.size() : count;
				v.resize(count);
				std::copy(end()-n, end(), v.begin());
				m_next = n;
				m_bufferSize = bufferSize;
				m_values.swap(v);
				m_next %= m_values.size();
			}
		}
		else
		{
			// decrease buffer size
			if (m_values.size() <= bufferSize)
			{
				// buffer not filled yet, so just shrink it
				m_bufferSize = bufferSize;
				m_values.reserve(bufferSize);
				if (m_values.size() < count)
					m_values.resize(count, value);
			}
			else
			{
				// need to move the elements to keep age order
				vector_type v;
				v.reserve(bufferSize);
				v.resize(bufferSize);
				std::copy(end()-bufferSize, end(), v.begin());
				m_bufferSize = bufferSize;
				m_values.swap(v);
				m_next = 0;
			}
		}
	}

	template<typename T, typename A> inline
	typename circular_buffer<T, A>::iterator circular_buffer<T, A>::begin()
	{
		return iterator(this, 0);
	}

	template<typename T, typename A> inline
	typename circular_buffer<T, A>::const_iterator circular_buffer<T, A>::begin() const
	{
		return const_iterator(this, 0);
	}

	template<typename T, typename A> inline
	typename circular_buffer<T, A>::iterator circular_buffer<T, A>::end()
	{
		return iterator(this, m_values.size());
	}

	template<typename T, typename A> inline
	typename circular_buffer<T, A>::const_iterator circular_buffer<T, A>::end() const
	{
		return const_iterator(this, m_values.size());
	}

	template<typename T, typename A> inline
	typename circular_buffer<T, A>::reverse_iterator circular_buffer<T, A>::rbegin()
	{
		return reverse_iterator(end());
	}

	template<typename T, typename A> inline
	typename circular_buffer<T, A>::const_reverse_iterator circular_buffer<T, A>::rbegin() const
	{
		return const_reverse_iterator(end());
	}

	template<typename T, typename A> inline
	typename circular_buffer<T, A>::reverse_iterator circular_buffer<T, A>::rend()
	{
		return reverse_iterator(begin());
	}

	template<typename T, typename A> inline
	typename circular_buffer<T, A>::const_reverse_iterator circular_buffer<T, A>::rend() const
	{
		return const_reverse_iterator(begin());
	}

}


#endif // __CIRCULARBUFFER_H__
